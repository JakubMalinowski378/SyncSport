using Shared.Domain;

namespace Reservations.Shared.Events;

public record ReservationCreatedEvent(Guid ReservationId) : IDomainEvent;
