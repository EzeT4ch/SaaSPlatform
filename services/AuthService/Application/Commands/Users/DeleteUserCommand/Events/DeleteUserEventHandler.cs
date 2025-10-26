using AuthService.Domain.Events.Users;
using Shared;

namespace AuthService.Application.Commands.Users.DeleteUserCommand.Events;

internal sealed class DeleteUserEventHandler : IDomainEventHandler<Domain.Events.Users.DeleteUserEvent>
{
    public Task Handle(DeleteUserEvent domainEvent, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}