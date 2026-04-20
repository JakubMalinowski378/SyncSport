using Reservations.Domain.Enums;
using Reservations.Domain.ValueObjects;
using Shared.Domain;

namespace Reservations.Domain.Entities;

public partial class Reservation : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CourtId { get; private set; }
    public TimeRange Time { get; private set; }
    public ReservationStatus Status { get; private set; }
    public decimal Price { get; private set; }
}
