using Facilities.Shared.DTOs;
namespace Facilities.Shared.DTOs;

public record FacilityAvailabilityDto(
    Guid FacilityId,
    IEnumerable<CourtAvailabilityInfo> Courts,
    IEnumerable<OpeningHoursAvailabilityInfo> OpeningHours);
