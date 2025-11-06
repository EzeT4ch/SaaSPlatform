using Microsoft.AspNetCore.Authorization;

namespace AuthService.Infrastructure.Segurity.Authorization;

internal sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
