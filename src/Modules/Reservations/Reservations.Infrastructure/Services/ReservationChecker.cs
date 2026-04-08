using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Reservations.Domain.Services;
using Shared.Persistence;

namespace Reservations.Infrastructure.Services;

internal sealed class ReservationChecker(
    IRepository<Reservation, Guid> reservationRepository)
    : IReservationChecker
{
    public Task<bool> IsUserHasConcurrentReservationAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        return reservationRepository.AnyAsync(r =>
            r.UserId == userId &&
            r.Status != ReservationStatus.Cancelled &&
            r.Time.Start < end &&
            r.Time.End > start, cancellationToken);
    }

    public async Task<bool> IsCourtAvailableAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default)
    {
        var hasConflict = await reservationRepository.AnyAsync(r =>
            r.CourtId == courtId &&
            r.Status != ReservationStatus.Cancelled &&
            r.Time.Start < end &&
            r.Time.End > start, cancellationToken);

        return !hasConflict;
    }
}