namespace Facilities.Shared.DTOs;

public record FacilityAvailabilityDto(
    Guid FacilityId,
    string FacilityName,
    IEnumerable<CourtAvailabilityInfo> Courts,
    IEnumerable<OpeningHoursAvailabilityInfo> OpeningHours);
