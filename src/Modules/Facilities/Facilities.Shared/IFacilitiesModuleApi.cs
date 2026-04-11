namespace Facilities.Shared;

public interface IFacilitiesModuleApi
{
    Task<Guid?> GetFacilityIdByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default);
    Task<FacilityAvailabilityDto?> GetFacilityAvailabilityInfoAsync(Guid facilityId, CancellationToken cancellationToken = default);
}

public record FacilityAvailabilityDto(
    Guid FacilityId,
    IEnumerable<CourtAvailabilityInfo> Courts,
    IEnumerable<OpeningHoursAvailabilityInfo> OpeningHours);

public record CourtAvailabilityInfo(Guid CourtId, string Name);
public record OpeningHoursAvailabilityInfo(DayOfWeek DayOfWeek, TimeSpan OpenTime, TimeSpan CloseTime);