namespace BillingService.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    // INFO: Include DbSet<Entity> entity { get; }
    
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}