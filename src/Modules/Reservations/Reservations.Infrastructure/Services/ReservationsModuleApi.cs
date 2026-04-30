using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
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

    public async Task<bool> UpdateStatusAsync(Guid reservationId, string newStatus, CancellationToken cancellationToken = default)
    {
        var reservation = await reservationRepository.GetByIdAsync(
            reservationId,
            asNoTracking: false,
            ct: cancellationToken);

        if (reservation is null)
        {
            return false;
        }

        if (!Enum.TryParse<ReservationStatus>(newStatus, out var status))
        {
            return false;
        }

        switch (status)
        {
            case ReservationStatus.Paid:
                reservation.MarkAsPaid();
                break;
            case ReservationStatus.Cancelled:
                reservation.Cancel();
                break;
            default:
                return false;
        }

        reservationRepository.Update(reservation);
        await reservationRepository.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<int> CancelStalePendingAsync(TimeSpan olderThan, CancellationToken cancellationToken = default)
    {
        var cutoff = DateTime.UtcNow - olderThan;

        var staleReservations = await reservationRepository.FindAsync(
            r => r.Status == ReservationStatus.Pending && r.CreatedAt < cutoff,
            asNoTracking: false,
            ct: cancellationToken);

        var count = 0;
        foreach (var reservation in staleReservations)
        {
            reservation.Cancel();
            reservationRepository.Update(reservation);
            count++;
        }

        if (count > 0)
        {
            await reservationRepository.SaveChangesAsync(cancellationToken);
        }

        return count;
    }
}
