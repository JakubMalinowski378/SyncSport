using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Shared.Pagination;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Queries.GetMyReservations;

public sealed record ReservationWithDetailsDto(
    Guid Id,
    Guid CourtId,
    string CourtName,
    string FacilityName,
    DateTime StartTime,
    DateTime EndTime,
    decimal Price,
    ReservationStatus Status);

public interface IReservationRepository : IRepository<Reservation, Guid>
{
    Task<PagedResult<ReservationWithDetailsDto>> GetMyReservationsWithDetailsAsync(
        Guid userId,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
}
