namespace Reservations.Domain.Services;

public interface IReservationChecker
{
    Task<bool> IsUserHasConcurrentReservationAsync(Guid userId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
    Task<bool> IsCourtAvailableAsync(Guid courtId, DateTimeOffset start, DateTimeOffset end, CancellationToken cancellationToken = default);
}
