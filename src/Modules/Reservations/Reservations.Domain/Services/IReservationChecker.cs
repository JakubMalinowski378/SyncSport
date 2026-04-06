namespace Reservations.Domain.Services;

public interface IReservationChecker
{
    Task<bool> IsUserHasConcurrentReservationAsync(Guid userId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
    Task<bool> IsCourtAvailableAsync(Guid courtId, DateTime start, DateTime end, CancellationToken cancellationToken = default);
}
