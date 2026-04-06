using Reservations.Domain.ValueObjects;
using Shared.Domain;

namespace Reservations.Domain.Entities;

public partial class Reservation : AggregateRoot<Guid>
{
    public Guid UserId { get; private set; }
    public Guid CourtId { get; private set; }
    public TimeRange Time { get; private set; }

    private Reservation() { Time = null!; }

    private Reservation(Guid id, Guid userId, Guid courtId, TimeRange time) : base(id)
    {
        UserId = userId;
        CourtId = courtId;
        Time = time;
    }
}
