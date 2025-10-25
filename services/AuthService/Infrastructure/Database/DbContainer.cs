using AuthService.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Database;

public sealed class DbContainer(
    DbContextOptions<DbContainer> options)
    : IdentityDbContext<User, Role, Guid>(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    // TODO: Implement feature flags
    //public DbSet<FeatureFlagOverride> FeatureFlags => Set<FeatureFlagOverride>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DbContainer).Assembly);

        modelBuilder.HasDefaultSchema(Schemas.Default);
    }
}