using Reservations.Domain.Enums;
using Reservations.Domain.ValueObjects;

namespace Reservations.Domain.Entities;

public partial class Reservation
{
    private Reservation() { Time = null!; }

    private Reservation(Guid id,
        Guid userId,
        Guid courtId,
        TimeRange time,
        ReservationStatus status,
        decimal price,
        DateTime createdAt)
        : base(id)
    {
        UserId = userId;
        CourtId = courtId;
        Time = time;
        Status = status;
        Price = price;
        CreatedAt = createdAt;
    }

    public static Reservation Create(Guid userId, Guid courtId, TimeRange time, decimal price)
    {
        var reservation = new Reservation(Guid.NewGuid(), userId, courtId, time, ReservationStatus.Pending, price, DateTime.UtcNow);
        return reservation;
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

    public void MarkAsAwaitingOnSitePayment()
    {
        if (Status is ReservationStatus.Cancelled or ReservationStatus.Paid)
        {
            throw new InvalidOperationException("Cannot mark a cancelled or already paid reservation for on-site payment.");
        }

        Status = ReservationStatus.AwaitingOnSitePayment;
    }

    public void MarkAsPaidOnSite()
    {
        if (Status != ReservationStatus.AwaitingOnSitePayment)
        {
            throw new InvalidOperationException("Only reservations awaiting on-site payment can be marked as paid on site.");
        }

        Status = ReservationStatus.Paid;
    }
}
