using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Database.Entities;

public class Role : IdentityRole<Guid>
{
    public Guid TenantId { get; set; }
    public string? Description { get; set; }
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}