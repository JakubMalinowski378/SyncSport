using Reservations.Shared.DTOs;

namespace Reservations.Shared;

public interface IReservationsModuleApi
{
    Task<ReservationDetailsDto?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);

    Task<bool> UpdateStatusAsync(Guid reservationId, string newStatus, CancellationToken cancellationToken = default);

    Task<int> CancelStalePendingAsync(TimeSpan olderThan, CancellationToken cancellationToken = default);
}
