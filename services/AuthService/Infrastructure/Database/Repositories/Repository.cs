using System.Linq.Expressions;
using AuthService.Infrastructure.Database.Extensions;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Database.Repositories;

public class Repository<TModel, TDomain> : IRepository<TModel, TDomain> where TModel : class where TDomain : class
{
    private readonly DbContainer _context;
    private readonly DbSet<TModel> _dbSet;
    private readonly Func<TModel, TDomain> _toDomain;
    private readonly Func<TDomain, TModel> _toModel;
    
    public Repository(DbContainer context, Func<TModel, TDomain> toDomain, Func<TDomain, TModel> toModel)
    {
        _context = context;
        _dbSet = _context.Set<TModel>();
        _toDomain = toDomain;
        _toModel = toModel;
    }

    public Task<TDomain?> GetByIdAsync(Guid id, Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
        bool isTracking = false, CancellationToken cancellationToken = default)
        => _dbSet.WithTracking(isTracking)
            .Where(x => EF.Property<Guid>(x, "Id") == id)
            .ApplyIncludes(include)
            .Select(x => _toDomain(x))
            .FirstOrDefaultAsync(cancellationToken);

    public async Task<IEnumerable<TDomain>> GetAllAsync(CancellationToken cancellationToken = default,
        int take = 100, int page = 1, Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
        bool isTracking = false)
        => await _dbSet.WithTracking(isTracking)
            .Skip((page - 1) * take)
            .Take(take)
            .ApplyIncludes(include)
            .Select(x => _toDomain(x))
            .ToListAsync(cancellationToken);

    public async Task<IEnumerable<TDomain>> FindAsync(Expression<Func<TModel, bool>> predicate,
        Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
        bool isTracking = false,
        CancellationToken cancellationToken = default)
        => await _dbSet.WithTracking(isTracking)
            .Where(predicate)
            .ApplyIncludes(include)
            .Select(x => _toDomain(x))
            .ToListAsync(cancellationToken);

    public Task AddAsync(TDomain entity, CancellationToken cancellationToken = default)
        => _dbSet.AddAsync(_toModel(entity), cancellationToken).AsTask();

    public Task AddRangeAsync(IEnumerable<TDomain> entities, CancellationToken cancellationToken = default)
        => _dbSet.AddRangeAsync(entities.Select(x => _toModel(x)), cancellationToken);

    public void Update(TDomain entity, CancellationToken cancellationToken = default)
        => _dbSet.Update(_toModel(entity));

    public void Remove(TDomain entity, CancellationToken cancellationToken = default)
        => _dbSet.Remove(_toModel(entity));

    public void RemoveRange(IEnumerable<TDomain> entities)
        => _dbSet.RemoveRange(entities.Select(_toModel));

    public IQueryable<TModel> AsQueryable(bool trackChanges = false)
        => trackChanges
            ? _dbSet.AsQueryable()
            : _dbSet.AsQueryable().AsNoTracking();

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _context.SaveChangesAsync(cancellationToken);
}