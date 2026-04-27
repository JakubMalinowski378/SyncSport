using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;

internal sealed class GetCourtReservationsQueryHandler(
    IRepository<Reservation, Guid> reservationRepository)
    : IRequestHandler<GetCourtReservationsQuery, GetCourtReservationsResponse>
{
    public async Task<GetCourtReservationsResponse> Handle(GetCourtReservationsQuery request, CancellationToken cancellationToken)
    {
        var weekStartDate = GetWeekStartDate(request.WeekDate);
        var weekEndExclusive = weekStartDate.AddDays(7);

        var reservations = await reservationRepository.FindAsync(
            r => r.CourtId == request.CourtId &&
                 r.Time.Start < weekEndExclusive &&
                 r.Time.End > weekStartDate,
            asNoTracking: true,
            ct: cancellationToken);

        var reservationsByDate = reservations
            .GroupBy(r => r.Time.Start.Date)
            .ToDictionary(
                g => g.Key,
                g => (IReadOnlyCollection<CourtReservationResponse>)g
                    .OrderBy(r => r.Time.Start)
                    .Select(r => new CourtReservationResponse(r.Id, r.Time.Start, r.Time.End, r.Status))
                    .ToList());

        var days = Enumerable
            .Range(0, 7)
            .Select(offset => weekStartDate.AddDays(offset))
            .Select(dayDate => new CourtReservationDayResponse(
                dayDate,
                dayDate.DayOfWeek,
                reservationsByDate.GetValueOrDefault(dayDate, [])))
            .ToList();

        return new GetCourtReservationsResponse(weekStartDate, weekStartDate.AddDays(6), days);
    }

    private static DateTime GetWeekStartDate(DateTime date)
    {
        var daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.Date.AddDays(-daysFromMonday);
    }
}
