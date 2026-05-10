using Shared.Domain;

namespace Facilities.Shared.Events;

public record FacilityCreatedEvent(Guid FacilityId) : IDomainEvent;