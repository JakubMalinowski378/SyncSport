using Shared.Domain;

namespace Users.Shared.Events;

public record UserActivatedEvent(Guid UserId) : IDomainEvent;