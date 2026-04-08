using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;

internal sealed class GetCourtReservationsQueryHandler(
    IRepository<Reservation, Guid> reservationRepository)
    : IRequestHandler<GetCourtReservationsQuery, IReadOnlyCollection<CourtReservationResponse>>
{
    public async Task<IReadOnlyCollection<CourtReservationResponse>> Handle(GetCourtReservationsQuery request, CancellationToken cancellationToken)
    {
        var reservations = await reservationRepository.FindAsync(
            r => r.CourtId == request.CourtId &&
                 (!request.StartDate.HasValue || r.Time.Start >= request.StartDate.Value) &&
                 (!request.EndDate.HasValue || r.Time.End <= request.EndDate.Value),
            asNoTracking: true,
            ct: cancellationToken);

        return reservations
            .OrderBy(r => r.Time.Start)
            .Select(r => new CourtReservationResponse(r.Id, r.Time.Start, r.Time.End, r.Status))
            .ToList();
    }
}
