using MediatR;
using Reservations.Domain.Enums;
using Shared.Pagination;

namespace Reservations.Application.Reservations.Queries.GetReservationsByUserId;

public record UserReservationResponse(Guid Id, Guid CourtId, DateTime StartTime, DateTime EndTime, ReservationStatus Status);
public record GetReservationsByUserIdQuery(Guid UserId, int PageNumber = 1, int PageSize = 10) 
    : IRequest<PagedResult<UserReservationResponse>>;
