using MediatR;

namespace Reservations.Application.Reservations.Queries.GetReservation;

public record ReservationDetailsResponse(Guid Id, Guid UserId, Guid CourtId, DateTime StartTime, DateTime EndTime);

public record GetReservationQuery(Guid Id) : IRequest<ReservationDetailsResponse?>;