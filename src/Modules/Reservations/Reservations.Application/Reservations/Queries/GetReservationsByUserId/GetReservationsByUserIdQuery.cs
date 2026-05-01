using MediatR;
using FluentValidation;
using Reservations.Domain.Enums;
using Shared.FluentValidation;
using Shared.Pagination;

namespace Reservations.Application.Reservations.Queries.GetReservationsByUserId;

public record UserReservationResponse(Guid Id, Guid CourtId, DateTime StartTime, DateTime EndTime, ReservationStatus Status);

public record GetReservationsByUserIdQuery(Guid UserId, int PageNumber = 1, int PageSize = 10)
    : IRequest<PagedResult<UserReservationResponse>>, IPaginatedRequest;

internal sealed class GetReservationsByUserIdQueryValidator : AbstractValidator<GetReservationsByUserIdQuery>
{
    public GetReservationsByUserIdQueryValidator()
    {
        this.AddPaginationRules();
        RuleFor(x => x.UserId).NotEmpty();
    }
}
