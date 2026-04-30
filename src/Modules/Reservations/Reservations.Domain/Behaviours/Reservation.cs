using Reservations.Domain.Enums;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
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

    public static async Task<Reservation> CreateAsync(
        Guid id,
        Guid userId,
        Guid courtId,
        DateTime start,
        DateTime end,
        decimal price,
        DateTime now,
        IReservationChecker reservationChecker,
        CancellationToken cancellationToken = default)
    {
        var time = TimeRange.Create(start, end);

        if (start < now)
        {
            throw new ReservationInPastException();
        }

        var isUserOccupied = await reservationChecker.IsUserHasConcurrentReservationAsync(userId, start, end, cancellationToken);
        if (isUserOccupied)
        {
            throw new UserAlreadyHasReservationException();
        }

        var isCourtAvailable = await reservationChecker.IsCourtAvailableAsync(courtId, start, end, cancellationToken);
        if (!isCourtAvailable)
        {
            throw new ReservationOverlapException();
        }

        return new Reservation(id, userId, courtId, time, ReservationStatus.Pending, price, now);
    }

    public static Reservation Create(Guid userId, Guid courtId, TimeRange time, decimal price)
    {
        var reservation = new Reservation(Guid.NewGuid(), userId, courtId, time, ReservationStatus.Pending, price, DateTime.UtcNow);
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
