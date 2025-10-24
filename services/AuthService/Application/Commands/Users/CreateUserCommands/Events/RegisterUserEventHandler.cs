using AuthService.Domain.Events.Users;
using Shared;

namespace AuthService.Application.Commands.Users.CreateUserCommands.Events;

internal sealed class RegisterUserEventHandler : IDomainEventHandler<RegisterUserEvent>
{
    public Task Handle(RegisterUserEvent domainEvent, CancellationToken cancellationToken)
    {
        // TODO: Implementar envio de mail para confirmar al usuario

        return Task.CompletedTask;
    }
}
