using Shared;

namespace AuthService.Domain.Events.Users;

public sealed record DeleteUserEvent(Guid Id) : IDomainEvent;
