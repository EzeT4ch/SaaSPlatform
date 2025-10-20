using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Database.Entities;

public class User : IdentityUser<Guid>
{
    public Guid TenantId { get; set; }
    public bool IsActive { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}