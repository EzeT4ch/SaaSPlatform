using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AuthService.Infrastructure.Database.Configurations;

public class UserRolesConfiguration : IEntityTypeConfiguration<UserRolesConfiguration>
{
    public void Configure(EntityTypeBuilder<UserRolesConfiguration> builder)
    {
        builder.ToTable("UserRoles");
    }
}