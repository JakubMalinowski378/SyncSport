using MediatR;
using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;   

public record CourtReservationResponse(Guid Id, DateTime StartTime, DateTime EndTime, ReservationStatus Status);
public record CourtReservationDayResponse(
    DateTime Date,
    DayOfWeek DayOfWeek,
    IReadOnlyCollection<CourtReservationResponse> Reservations);

public record GetCourtReservationsResponse(
    DateTime WeekStartDate,
    DateTime WeekEndDate,
    IReadOnlyCollection<CourtReservationDayResponse> Days);

public record GetCourtReservationsQuery(Guid CourtId, DateTime WeekDate)
    : IRequest<GetCourtReservationsResponse>;
