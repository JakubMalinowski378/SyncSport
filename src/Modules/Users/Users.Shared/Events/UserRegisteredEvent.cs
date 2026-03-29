using Shared.Domain;

namespace Users.Shared.Events;

public record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent;
