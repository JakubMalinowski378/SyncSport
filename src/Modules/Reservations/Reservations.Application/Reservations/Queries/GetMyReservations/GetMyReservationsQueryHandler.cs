using MediatR;
using Shared.Pagination;

namespace Reservations.Application.Reservations.Queries.GetMyReservations;

internal sealed class GetMyReservationsQueryHandler(
    IReservationRepository reservationRepository)
    : IRequestHandler<GetMyReservationsQuery, PagedResult<ReservationWithDetailsDto>>
{
    public async Task<PagedResult<ReservationWithDetailsDto>> Handle(
        GetMyReservationsQuery request,
        CancellationToken cancellationToken)
    {
        return await reservationRepository.GetMyReservationsWithDetailsAsync(
            request.UserId,
            request.PageNumber,
            request.PageSize,
            cancellationToken);
    }
}
