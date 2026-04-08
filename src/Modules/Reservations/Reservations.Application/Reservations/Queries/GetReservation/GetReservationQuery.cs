using MediatR;
using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations.Queries.GetReservation;

public record ReservationDetailsResponse(Guid Id, Guid UserId, Guid CourtId, DateTime StartTime, DateTime EndTime, ReservationStatus Status);

public record GetReservationQuery(Guid Id) : IRequest<ReservationDetailsResponse?>;