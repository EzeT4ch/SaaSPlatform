using Shared;
using AuthService.Domain.Enums;

namespace AuthService.Domain.Entities;

public class User : Entity
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string UserName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public bool PhoneNumberConfirmed { get; set; }

    public Guid TenantId { get; set; }

    public string PasswordHash { get; set; } = null!;
    public string FullName { get; set; } = null!;
    public UserRole Role { get; set; } = UserRole.Client;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  
    public Tenant Tenant { get; set; } = null!;

    public void AssignRole(string role)
    {
        Role = role.ToLower() switch
        {
            "admin" => UserRole.Admin,
            "client" => UserRole.Client,
            _ => throw new ArgumentException("Invalid role", nameof(role))
        };
    }
    
    public void Deactivate()
    {
        IsActive = false;
    }
}
