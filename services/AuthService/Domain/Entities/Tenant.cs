using Shared;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class Tenant : Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = null!;
    public string? Domain { get; set; }
    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public TenantStatus Status { get; set; } = TenantStatus.Active;

    public ICollection<User> Users { get; set; } = [];
}
