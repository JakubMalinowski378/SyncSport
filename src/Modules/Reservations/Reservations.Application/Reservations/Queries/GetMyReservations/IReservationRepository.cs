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
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    decimal Price,
    ReservationStatus Status);

public sealed record ReservationFilters(
    ReservationStatus? Status);

public sealed record GetMyReservationsResult(
    IReadOnlyList<Reservation> Reservations,
    Dictionary<Guid, CourtWithDetails> CourtDetails);

public sealed record CourtWithDetails(
    string CourtName,
    string FacilityName);

public interface IReservationRepository : IRepository<Reservation, Guid>
{
    Task<IReadOnlyList<Reservation>> GetMyReservationsAsync(
        Guid userId,
        ReservationFilters filters,
        CancellationToken cancellationToken = default);
}
