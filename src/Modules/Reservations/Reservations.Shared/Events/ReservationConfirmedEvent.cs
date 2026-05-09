using Shared.Domain;

namespace Reservations.Shared.Events;

public record ReservationConfirmedEvent(Guid ReservationId) : IDomainEvent;