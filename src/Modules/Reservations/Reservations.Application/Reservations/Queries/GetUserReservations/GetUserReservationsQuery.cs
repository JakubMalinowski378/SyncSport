using System.Text.Json.Serialization;
using MediatR;
using Reservations.Domain.Enums;
using Shared.Pagination;

namespace Reservations.Application.Reservations.Queries.GetUserReservations;

public record ReservationResponse(Guid Id, Guid CourtId, DateTime StartTime, DateTime EndTime, ReservationStatus Status);

public sealed class GetUserReservationsQuery
    : IRequest<PagedResult<ReservationResponse>>
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}
