using MediatR;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;

public record CourtReservationResponse(Guid Id, DateTime StartTime, DateTime EndTime);

public record GetCourtReservationsQuery(Guid CourtId, DateTime? StartDate, DateTime? EndDate) 
    : IRequest<IReadOnlyCollection<CourtReservationResponse>>;
