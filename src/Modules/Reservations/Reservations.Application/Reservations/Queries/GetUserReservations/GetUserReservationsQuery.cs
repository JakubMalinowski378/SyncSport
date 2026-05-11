using FluentValidation;
using MediatR;
using Reservations.Domain.Enums;
using Shared.FluentValidation;
using Shared.Pagination;
using System.Text.Json.Serialization;

namespace Reservations.Application.Reservations.Queries.GetUserReservations;

public record ReservationResponse(Guid Id, Guid CourtId, DateTimeOffset StartTime, DateTimeOffset EndTime, ReservationStatus Status);

public sealed class GetUserReservationsQuery
    : IRequest<PagedResult<ReservationResponse>>, IPaginatedRequest
{
    [JsonIgnore]
    public Guid UserId { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
}

internal sealed class GetUserReservationsQueryValidator : AbstractValidator<GetUserReservationsQuery>
{
    public GetUserReservationsQueryValidator()
    {
        this.AddPaginationRules();
    }
}
