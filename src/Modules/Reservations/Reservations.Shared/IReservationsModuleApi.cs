using Reservations.Shared.DTOs;

namespace Reservations.Shared;

public interface IReservationsModuleApi
{
    Task<ReservationDetailsDto?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken = default);
}
