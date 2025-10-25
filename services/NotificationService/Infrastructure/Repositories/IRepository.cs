using System.Linq.Expressions;

namespace NotificationService.Infrastructure.Repositories;


public interface IRepository<TModel, TDomain> where TDomain : class where TModel : class
{
    Task<TDomain?> GetByIdAsync(int id, Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
        bool isTracking = false, CancellationToken cancellationToken = default);

    Task<IEnumerable<TDomain>> GetAllAsync(CancellationToken cancellationToken = default,
        int take = 100, int page = 1, Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
        bool isTracking = false);

    Task<IEnumerable<TDomain>> FindAsync(Expression<Func<TModel, bool>> predicate,
        Func<IQueryable<TModel>, IQueryable<TModel>>? include = null,
        bool isTracking = false,
        CancellationToken cancellationToken = default);

    Task AddAsync(TDomain entity, CancellationToken cancellationToken = default);

    Task AddRangeAsync(IEnumerable<TDomain> entities, CancellationToken cancellationToken = default);

    void Update(TDomain entity, CancellationToken cancellationToken = default);

    void Remove(TDomain entity, CancellationToken cancellationToken = default);

    void RemoveRange(IEnumerable<TDomain> entities);

    IQueryable<TModel> AsQueryable(bool trackChanges = false);

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
