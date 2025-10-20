using AuthService.Infrastructure.Database.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace AuthService.Infrastructure.Database.Identity;

public class ApplicationUserStore(DbContainer context) : UserStore<User, Role, DbContainer, Guid>(context)
{
    public override Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        if (user.TenantId == Guid.Empty)
            throw new InvalidOperationException("TenantId is required for multi-tenant users.");

        return base.CreateAsync(user, cancellationToken);
    }

    public override IQueryable<User> Users 
        => base.Users.Where(u => u.IsActive);
}