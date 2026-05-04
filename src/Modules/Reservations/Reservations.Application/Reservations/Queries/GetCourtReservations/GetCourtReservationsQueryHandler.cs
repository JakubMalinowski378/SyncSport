using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;
using Shared.Time;

namespace Reservations.Application.Reservations.Queries.GetCourtReservations;

internal sealed class GetCourtReservationsQueryHandler(
    IRepository<Reservation, Guid> reservationRepository,
    Facilities.Shared.IFacilitiesModuleApi facilitiesModuleApi)
    : IRequestHandler<GetCourtReservationsQuery, GetCourtReservationsResponse>
{
    public async Task<GetCourtReservationsResponse> Handle(GetCourtReservationsQuery request, CancellationToken cancellationToken)
    {
        var weekStartDate = GetWeekStartDate(request.WeekDate);
        var weekEndExclusive = weekStartDate.AddDays(7);

        var weekStartUtc = PolishTimeProvider.PolishMidnightToUtc(weekStartDate);
        var weekEndUtc = PolishTimeProvider.PolishMidnightToUtc(weekEndExclusive);

        var reservations = await reservationRepository.FindAsync(
            r => r.CourtId == request.CourtId &&
                 r.Time.Start < weekEndUtc &&
                 r.Time.End > weekStartUtc,
            asNoTracking: true,
            ct: cancellationToken);

        var facilityId = await facilitiesModuleApi.GetFacilityIdByCourtIdAsync(request.CourtId, cancellationToken);
        if (facilityId is null)
            throw new Exception("Facility for court not found.");

        var facilityInfo = await facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId.Value, cancellationToken);
        if (facilityInfo is null)
            throw new Exception("Facility availability info not found.");

        var courtInfo = facilityInfo.Courts.FirstOrDefault(c => c.CourtId == request.CourtId);
        if (courtInfo is null)
            throw new Exception("Court not found in facility info.");

        var reservationDurationMinutes = courtInfo.ReservationDurationMinutes;

        var reservationsList = reservations.OrderBy(r => r.Time.Start).ToList();

        var days = Enumerable.Range(0, 7)
            .Select(offset => weekStartDate.AddDays(offset))
            .Select(dayDate =>
            {
                var dayDateTime = PolishTimeProvider.PolishMidnightToUtc(dayDate);
                var dayReservations = reservationsList.Where(r => r.Time.Start.Date == dayDateTime.Date).ToList();

                var openingHours = facilityInfo.OpeningHours.FirstOrDefault(h => h.DayOfWeek == dayDate.DayOfWeek);

                var slots = new List<CourtReservationSlotResponse>();

                if (openingHours != null && reservationDurationMinutes > 0)
                {
                    var current = dayDateTime.Add(openingHours.OpenTime);
                    var endTime = dayDateTime.Add(openingHours.CloseTime);
                    var slotDuration = TimeSpan.FromMinutes(reservationDurationMinutes);

                    while (current.Add(slotDuration) <= endTime)
                    {
                        var slotEnd = current.Add(slotDuration);

                        var overlapping = dayReservations.FirstOrDefault(r => r.Status != Domain.Enums.ReservationStatus.Cancelled && r.Time.Start < slotEnd && r.Time.End > current);

                        if (overlapping is null)
                        {
                            slots.Add(new CourtReservationSlotResponse(current, slotEnd, null));
                            current = current.AddMinutes(reservationDurationMinutes);
                        }
                        else
                        {
                            var slotStatus = overlapping.Status switch
                            {
                                Domain.Enums.ReservationStatus.Pending => "Pending",
                                Domain.Enums.ReservationStatus.Paid or Domain.Enums.ReservationStatus.AwaitingOnSitePayment => "reserved",
                                _ => null
                            };

                            slots.Add(new CourtReservationSlotResponse(overlapping.Time.Start, overlapping.Time.End, slotStatus));

                            current = overlapping.Time.End > current ? overlapping.Time.End : current.AddMinutes(reservationDurationMinutes);
                        }
                    }
                }

                return new CourtReservationDayResponse(dayDateTime, dayDate.DayOfWeek, slots);
            })
            .ToList();

        return new GetCourtReservationsResponse(weekStartUtc, PolishTimeProvider.PolishMidnightToUtc(weekStartDate.AddDays(6)), days);
    }

    private static DateOnly GetWeekStartDate(DateOnly date)
    {
        var daysFromMonday = ((int)date.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        return date.AddDays(-daysFromMonday);
    }
}
