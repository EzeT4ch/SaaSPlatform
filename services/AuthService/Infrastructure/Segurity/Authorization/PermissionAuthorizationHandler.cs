using AuthService.Infrastructure.Segurity.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace AuthService.Infrastructure.Segurity.Authorization;

internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User?.Identity is not { IsAuthenticated: true })
            return;
        
        if (context.User.IsInRole("Admin"))
        {
            context.Succeed(requirement);
            return;
        }
        
        string? userIdClaim = context.User.FindFirst("sub")?.Value;
        if (userIdClaim is null || !Guid.TryParse(userIdClaim, out Guid userId))
            return;

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);

        if (permissions.Contains(requirement.Permission) || permissions.Contains("*"))
        {
            context.Succeed(requirement);
        }
    }
}
