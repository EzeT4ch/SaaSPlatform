using AuthService.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure.Segurity.Authorization;

internal sealed class PermissionProvider(UserManager<User> userManager, RoleManager<Role> roleManager)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId)
    {
        User? user = await userManager.FindByIdAsync(userId.ToString());

        if (user is null)
            return [];

        IList<string> roles = await userManager.GetRolesAsync(user);
        HashSet<string> permissions = new(StringComparer.OrdinalIgnoreCase);
        
        if (roles.Contains("Admin"))
        {
            permissions.Add("Admin");
            permissions.Add("*");
            return permissions;
        }
        
        foreach (string roleName in roles)
        {
            Role? role = await roleManager.Roles
                .Include(r => r.RolePermissions)
                .FirstOrDefaultAsync(r => r.Name == roleName);
            
            if (role == null)
                continue;

            if (!string.IsNullOrEmpty(role.ConcurrencyStamp))
            {
                foreach (string perm in role.ConcurrencyStamp.Split(',', StringSplitOptions.RemoveEmptyEntries))
                    permissions.Add(perm.Trim());
            }
        }

        return permissions;
    }
}
