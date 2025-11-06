namespace AuthService.Infrastructure.Database.Entities;

public class RolePermission
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public string Permission { get; set; } = null!;

    public Role Role { get; set; } = null!;
}