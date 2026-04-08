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

    private Reservation() { Time = null!; }

    private Reservation(Guid id, Guid userId, Guid courtId, TimeRange time, ReservationStatus status) : base(id)
    {
        UserId = userId;
        CourtId = courtId;
        Time = time;
        Status = status;
    }

    public static Reservation Create(Guid userId, Guid courtId, TimeRange time) 
    {
        var reservation = new Reservation(Guid.NewGuid(), userId, courtId, time, ReservationStatus.Pending);
        return reservation;
    }

    public void Confirm()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot confirm a cancelled reservation.");
        }
        Status = ReservationStatus.Confirmed;
    }

    public void MarkAsPaid()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Cannot pay for a cancelled reservation.");
        }
        Status = ReservationStatus.Paid;
    }

    public void Cancel()
    {
        if (Status == ReservationStatus.Cancelled)
        {
            throw new InvalidOperationException("Reservation is already cancelled.");
        }
        Status = ReservationStatus.Cancelled;
    }
}
