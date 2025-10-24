using AuthService.Application.Abstractions.Messaging;

namespace AuthService.Application.Commands.Users.CreateUserCommands;

public sealed record RegisterUserCommand(string email, string username, string password) : ICommand<Guid>;

