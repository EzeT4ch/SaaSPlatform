using Shared;

namespace AuthService.Domain.Events.Users;

public sealed record RegisterUserEvent(Guid UserId) : IDomainEvent;

