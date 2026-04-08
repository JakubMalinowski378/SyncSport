using MediatR;
using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;   

public record CourtReservationResponse(Guid Id, DateTime StartTime, DateTime EndTime, ReservationStatus Status);
public record GetCourtReservationsQuery(Guid CourtId, DateTime? StartDate, DateTime? EndDate) 
    : IRequest<IReadOnlyCollection<CourtReservationResponse>>;
