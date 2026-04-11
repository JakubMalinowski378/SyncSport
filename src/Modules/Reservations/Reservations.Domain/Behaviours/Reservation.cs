using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Reservations.Domain.Enums;
using Reservations.Domain.ValueObjects;

namespace Reservations.Domain.Entities;

public partial class Reservation
{
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

        return new Reservation(id, userId, courtId, time, ReservationStatus.Pending, price);
    }
}
