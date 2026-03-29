using Shared.Domain;

namespace Users.Shared.Events;

public record UserDeactivatedEvent(Guid UserId) : IDomainEvent;
