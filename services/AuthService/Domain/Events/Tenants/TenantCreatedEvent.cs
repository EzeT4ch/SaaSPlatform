using Shared;

namespace AuthService.Domain.Events.Tenants;

public sealed record TenantCreatedEvent(Guid TenantId, Guid AdminUserId) : IDomainEvent;