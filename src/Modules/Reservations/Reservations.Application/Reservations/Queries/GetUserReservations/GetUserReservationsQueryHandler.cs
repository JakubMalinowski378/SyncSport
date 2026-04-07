using MediatR;
using Reservations.Domain.Entities;
using Shared.Pagination;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Reservations.Queries.GetUserReservations;

internal sealed class GetUserReservationsQueryHandler(
    IRepository<Reservation, Guid> reservationRepository,
    ICurrentUser currentUser)
    : IRequestHandler<GetUserReservationsQuery, PagedResult<ReservationResponse>>
{
    public async Task<PagedResult<ReservationResponse>> Handle(GetUserReservationsQuery request, CancellationToken cancellationToken)
    {
        var currentUserState = currentUser.GetState();

        var result = await reservationRepository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            filter: r => r.UserId == currentUserState.UserId,
            orderBy: q => q.OrderByDescending(r => r.Time.Start),
            asNoTracking: true,
            ct: cancellationToken);

        var responseItems = result.Items.Select(r => new ReservationResponse(
            r.Id,
            r.CourtId,
            r.Time.Start,
            r.Time.End)).ToList();

        return new PagedResult<ReservationResponse>(
            responseItems, 
            result.TotalCount, 
            request.PageNumber, 
            request.PageSize);
    }
}
