using MediatR;
using Reservations.Domain.Enums;
using Shared.Pagination;

namespace Reservations.Application.Reservations.Queries.GetUserReservations;    

public record ReservationResponse(Guid Id, Guid CourtId, DateTime StartTime, DateTime EndTime, ReservationStatus Status);
public record GetUserReservationsQuery(int PageNumber = 1, int PageSize = 10) 
    : IRequest<PagedResult<ReservationResponse>>;
