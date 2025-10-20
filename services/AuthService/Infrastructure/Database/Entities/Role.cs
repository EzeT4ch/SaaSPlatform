using Microsoft.AspNetCore.Identity;

namespace AuthService.Infrastructure.Database.Entities;

public class Role : IdentityRole<Guid>
{
    public string? Description { get; set; }
}