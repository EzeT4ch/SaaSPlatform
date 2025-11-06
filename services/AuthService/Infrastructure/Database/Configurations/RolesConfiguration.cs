using AuthService.Infrastructure.Database.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Database.Configurations;

public class RolesConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.Property(r => r.TenantId)
            .IsRequired();

        builder.HasIndex(r => new { r.Name, r.TenantId })
            .IsUnique();
        
        builder.HasIndex(r => new { r.Name, r.TenantId }).IsUnique();
    }
}