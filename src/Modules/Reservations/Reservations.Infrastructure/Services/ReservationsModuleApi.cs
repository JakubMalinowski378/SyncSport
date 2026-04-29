using Reservations.Domain.Entities;
using Reservations.Shared;
using Reservations.Shared.DTOs;
using Shared.Persistence;

namespace Reservations.Infrastructure.Services;

internal sealed class ReservationsModuleApi(IRepository<Reservation, Guid> reservationRepository)
    : IReservationsModuleApi
{
    public async Task<ReservationDetailsDto?> GetByIdAsync(Guid reservationId, CancellationToken cancellationToken = default)
    {
        var reservation = await reservationRepository.GetByIdAsync(
            reservationId,
            asNoTracking: true,
            ct: cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        return new ReservationDetailsDto(
            reservation.Id,
            reservation.CourtId,
            reservation.UserId,
            reservation.Status.ToString(),
            reservation.Price,
            reservation.Time.Start,
            reservation.Time.End);
    }
}
