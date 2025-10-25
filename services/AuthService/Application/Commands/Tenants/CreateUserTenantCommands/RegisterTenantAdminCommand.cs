using AuthService.Application.Abstractions.Messaging;

namespace AuthService.Application.Commands.Tenants.CreateUserTenantCommands;

public sealed record RegisterTenantAdminCommand(
    string TenantName,
    string Email,
    string Password,
    string FullName,
    string Username
    ) : ICommand<Guid>;
