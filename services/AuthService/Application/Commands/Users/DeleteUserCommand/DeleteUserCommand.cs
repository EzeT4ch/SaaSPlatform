using AuthService.Application.Abstractions.Messaging;

namespace AuthService.Application.Commands.Users.DeleteUserCommand;

public sealed record DeleteUserCommand(Guid TenantId, string email) : ICommand;
