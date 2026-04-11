using Facilities.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Reservations.Domain.Entities;
using Reservations.Domain.Enums;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Queries.GetAvailableSlots;

internal sealed class GetAvailableSlotsQueryHandler(
    IFacilitiesModuleApi facilitiesModuleApi,
    IRepository<Reservation, Guid> reservationRepository)
    : IRequestHandler<GetAvailableSlotsQuery, AvailableSlotsResponse>
{
    public async Task<AvailableSlotsResponse> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var facilityInfo = await facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(request.FacilityId, cancellationToken);
        if (facilityInfo is null)
        {
            throw new Exception("Facility not found.");
        }

        var dayOfWeek = request.Date.DayOfWeek;
        var openingHours = facilityInfo.OpeningHours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek);

        var courtsResult = new List<CourtAvailabilityDto>();

        if (openingHours is null)
        {
            // Facility is closed on this day
            return new AvailableSlotsResponse(request.Date, courtsResult);
        }

        var courtIds = facilityInfo.Courts.Select(c => c.CourtId).ToList();
        
        var dateStart = request.Date.ToDateTime(TimeOnly.MinValue);
        var dateEnd = request.Date.ToDateTime(TimeOnly.MaxValue);

        var existingReservations = await reservationRepository.FindAsync(
            predicate: r => courtIds.Contains(r.CourtId) 
                         && r.Time.Start >= dateStart 
                         && r.Time.Start <= dateEnd
                         && r.Status != ReservationStatus.Cancelled,
            asNoTracking: true,
            ct: cancellationToken);

        var slotDuration = TimeSpan.FromHours(1);

        foreach (var court in facilityInfo.Courts)
        {
            var courtReservations = existingReservations.Where(r => r.CourtId == court.CourtId).ToList();
            var availableStartTimes = new List<TimeSpan>();

            var currentTime = openingHours.OpenTime;
            while (currentTime + slotDuration <= openingHours.CloseTime)
            {
                var slotStart = request.Date.ToDateTime(TimeOnly.FromTimeSpan(currentTime));
                var slotEnd = slotStart.Add(slotDuration);

                // For today, do not allow past slots
                if (slotStart <= DateTime.UtcNow)
                {
                    currentTime += slotDuration;
                    continue;
                }

                var isOverlapping = courtReservations.Any(r => r.Time.Start < slotEnd && r.Time.End > slotStart);
                
                if (!isOverlapping)
                {
                    availableStartTimes.Add(currentTime);
                }

                currentTime += slotDuration;
            }

            courtsResult.Add(new CourtAvailabilityDto(court.CourtId, court.Name, availableStartTimes));
        }

        return new AvailableSlotsResponse(request.Date, courtsResult);
    }
}