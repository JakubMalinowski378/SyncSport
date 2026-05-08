using Reservations.Application.Reservations.Queries.GetMyReservations;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Application.Common.Interfaces;

public interface IReservationRepository : IRepository<Reservation, Guid>
{
    Task<IReadOnlyList<Reservation>> GetMyReservationsAsync(
        Guid userId,
        ReservationFilters filters,
        CancellationToken cancellationToken = default);
}