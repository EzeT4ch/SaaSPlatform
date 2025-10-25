using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Shared;
using System.Data;
using WmsService.Infrastructure.Database;
using WmsService.Infrastructure.DomainEvents;

namespace WmsService.Infrastructure.Transactions;

public class UnitOfWork : IUnitOfWork
{
    private readonly IDomainEventsDispatcher _dispatcher;
    private readonly DbContainer _context;
    private IDbContextTransaction? _transaction;
    private readonly ILogger<UnitOfWork> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncTimeoutPolicy _timeoutPolicy;
    private readonly AsyncCircuitBreakerPolicy _breakerPolicy;
    public void Dispose()
        => _transaction?.Dispose();

    public UnitOfWork(DbContainer context, IDomainEventsDispatcher dispatcher, ILogger<UnitOfWork> logger)
    {
        _context = context;
        _dispatcher = dispatcher;
        _logger = logger;

        _retryPolicy = Policy
            .Handle<DbUpdateConcurrencyException>()
            .Or<DbUpdateException>(ex =>
                ex.InnerException?.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase) == true)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt =>
                    TimeSpan.FromMilliseconds(100 * Math.Pow(2, attempt) + Random.Shared.Next(0, 50)),
                onRetry: (ex, delay, attempt, _) =>
                    Console.WriteLine($"[UoW-Retry] intento {attempt} por {ex.GetType().Name} -> espera {delay.TotalMilliseconds}ms"));

        _timeoutPolicy = Policy.TimeoutAsync(5, TimeoutStrategy.Pessimistic);

        _breakerPolicy = Policy
            .Handle<Exception>()
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 10,
                durationOfBreak: TimeSpan.FromSeconds(10),
                onBreak: (ex, breakDelay) =>
                    Console.WriteLine($"[UoW] Circuito abierto ({ex.Message}), pausa {breakDelay.Seconds}s"),
                onReset: () => Console.WriteLine("[UoW] Circuito cerrado."),
                onHalfOpen: () => Console.WriteLine("[UoW] Probando recuperación..."));
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _breakerPolicy.WrapAsync(_timeoutPolicy)
            .WrapAsync(_retryPolicy)
            .ExecuteAsync(async ct =>
            {
                int changes = await _context.SaveChangesAsync(ct);

                await PublishDomainEventsAsync();

                return changes;
            }, cancellationToken);
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
            return;

        _transaction = await _context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);
    }

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        try
        {
            await _breakerPolicy.WrapAsync(_timeoutPolicy)
                .ExecuteAsync(async ct =>
                {
                    await _transaction.CommitAsync(ct);
                }, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[UoW] Error al hacer commit, intentando rollback...");
            await RollbackAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }


    public async Task RollbackAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
            return;

        await _transaction.RollbackAsync(cancellationToken);
        await _transaction.DisposeAsync();
        _transaction = null;
    }

    private async Task PublishDomainEventsAsync()
    {
        List<IDomainEvent> domainEvents = _context.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .SelectMany(e =>
            {
                List<IDomainEvent> events = e.DomainEvents.ToList();
                e.ClearDomainEvents();
                return events;
            })
            .ToList();

        if (domainEvents.Count == 0)
            return;

        await _dispatcher.DispatchAsync(domainEvents);
    }

}