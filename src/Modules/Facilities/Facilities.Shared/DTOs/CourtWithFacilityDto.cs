namespace Facilities.Shared.DTOs;

public sealed record CourtWithFacilityDto(
    Guid CourtId,
    string CourtName,
    Guid FacilityId,
    string FacilityName);
