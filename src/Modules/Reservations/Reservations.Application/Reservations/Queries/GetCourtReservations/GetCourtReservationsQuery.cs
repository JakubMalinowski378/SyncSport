using MediatR;
using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;

public record CourtReservationResponse(Guid Id, DateTime StartTime, DateTime EndTime, ReservationStatus Status);

public record CourtReservationSlotResponse(
    DateTime StartTime,
    DateTime EndTime,
    bool IsReserved);

public record CourtReservationDayResponse(
    DateTime Date,
    DayOfWeek DayOfWeek,
    IReadOnlyCollection<CourtReservationSlotResponse> Slots);

public record GetCourtReservationsResponse(
    DateOnly WeekStartDate,
    DateOnly WeekEndDate,
    IReadOnlyCollection<CourtReservationDayResponse> Days);

public record GetCourtReservationsQuery(Guid CourtId, DateOnly WeekDate)
    : IRequest<GetCourtReservationsResponse>;
