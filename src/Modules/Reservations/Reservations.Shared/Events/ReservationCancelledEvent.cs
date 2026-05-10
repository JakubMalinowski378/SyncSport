using Shared.Domain;

namespace Reservations.Shared.Events;

public record ReservationCancelledEvent(Guid ReservationId) : IDomainEvent;