using MediatR;
using Reservations.Domain.Entities;
using Shared.Pagination;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Queries.GetReservationsByUserId;

internal sealed class GetReservationsByUserIdQueryHandler(
    IRepository<Reservation, Guid> reservationRepository)
    : IRequestHandler<GetReservationsByUserIdQuery, PagedResult<UserReservationResponse>>
{
    public async Task<PagedResult<UserReservationResponse>> Handle(GetReservationsByUserIdQuery request, CancellationToken cancellationToken)
    {
        var result = await reservationRepository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            filter: r => r.UserId == request.UserId,
            orderBy: q => q.OrderByDescending(r => r.Time.Start),
            asNoTracking: true,
            ct: cancellationToken);

        var responseItems = result.Items.Select(r => new UserReservationResponse(
            r.Id,
            r.CourtId,
            r.Time.Start,
            r.Time.End,
            r.Status)).ToList();

        return new PagedResult<UserReservationResponse>(
            responseItems, 
            result.TotalCount, 
            request.PageNumber, 
            request.PageSize);
    }
}
