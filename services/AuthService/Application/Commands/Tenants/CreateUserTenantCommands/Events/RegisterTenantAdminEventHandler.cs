using AuthService.Domain.Events.Tenants;
using Shared;

namespace AuthService.Application.Commands.Tenants.CreateUserTenantCommands.Events;

internal sealed class RegisterTenantAdminEventHandler : IDomainEventHandler<TenantCreatedEvent>
{
    public Task Handle(TenantCreatedEvent domainEvent, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
