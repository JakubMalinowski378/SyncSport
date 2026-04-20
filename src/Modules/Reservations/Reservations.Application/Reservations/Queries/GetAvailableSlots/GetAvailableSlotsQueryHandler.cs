using Facilities.Shared;
using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Queries.GetAvailableSlots;

internal sealed class GetAvailableSlotsQueryHandler : IRequestHandler<GetAvailableSlotsQuery, GetAvailableSlotsResponse>
{
    private readonly IRepository<Reservation, Guid> _reservationRepository;
    private readonly IFacilitiesModuleApi _facilitiesModuleApi;

    public GetAvailableSlotsQueryHandler(
        IRepository<Reservation, Guid> reservationRepository,
        IFacilitiesModuleApi facilitiesModuleApi)
    {
        _reservationRepository = reservationRepository;
        _facilitiesModuleApi = facilitiesModuleApi;
    }

    public async Task<GetAvailableSlotsResponse> Handle(GetAvailableSlotsQuery request, CancellationToken cancellationToken)
    {
        var facilityInfo = await _facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(request.FacilityId, cancellationToken);
        if (facilityInfo is null)
        {
            throw new Exception("Facility not found.");
        }

        var courtExists = facilityInfo.Courts.Any(c => c.CourtId == request.CourtId);
        if (!courtExists)
        {
            throw new Exception("Court not found in facility.");
        }

        var dayOfWeek = request.Date.DayOfWeek;
        var openingHours = facilityInfo.OpeningHours.FirstOrDefault(h => h.DayOfWeek == dayOfWeek);

        var dateStart = request.Date.Date;
        var dateEnd = dateStart.AddDays(1);

        var reservationsTask = await _reservationRepository.FindAsync(
            r => r.CourtId == request.CourtId &&
                 r.Time.Start >= dateStart &&
                 r.Time.End <= dateEnd,
            asNoTracking: true,
            ct: cancellationToken);

        var reservations = reservationsTask
            .OrderBy(r => r.Time.Start)
            .ToList();

        var reservationMap = reservations
            .Select(r => new ReservationSlotResponse(r.Id, r.Time.Start, r.Time.End, r.Status))
            .ToList();

        var availableSlots = new List<AvailableSlotResponse>();

        if (openingHours != null)
        {
            var currentTime = dateStart.Add(openingHours.OpenTime);
            var endTime = dateStart.Add(openingHours.CloseTime);

            var slotDuration = TimeSpan.FromMinutes(60);

            while (currentTime.Add(slotDuration) <= endTime)
            {
                var slotEnd = currentTime.Add(slotDuration);
                
                var isOverlapping = reservations.Any(r => r.Status != Domain.Enums.ReservationStatus.Cancelled && r.Time.Start < slotEnd && r.Time.End > currentTime);

                if (!isOverlapping)
                {
                    availableSlots.Add(new AvailableSlotResponse(currentTime, slotEnd));
                }

                currentTime = currentTime.AddMinutes(30); // Advance by 30 mins to allow flexible start times
            }
        }

        return new GetAvailableSlotsResponse(reservationMap, availableSlots);
    }
}