using MediatR;
using Reservations.Domain.Enums;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;

public record CourtReservationResponse(Guid Id, DateTimeOffset StartTime, DateTimeOffset EndTime, ReservationStatus Status);

public record CourtReservationSlotResponse(
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    string? Status);

public record CourtReservationDayResponse(
    DateTimeOffset Date,
    DayOfWeek DayOfWeek,
    IReadOnlyCollection<CourtReservationSlotResponse> Slots);

public record GetCourtReservationsResponse(
    DateTimeOffset WeekStartDate,
    DateTimeOffset WeekEndDate,
    IReadOnlyCollection<CourtReservationDayResponse> Days);

public record GetCourtReservationsQuery(Guid CourtId, DateOnly WeekDate)
    : IRequest<GetCourtReservationsResponse>;
