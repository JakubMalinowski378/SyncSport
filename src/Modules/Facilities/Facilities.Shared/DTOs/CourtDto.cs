namespace Facilities.Shared.DTOs;

public sealed record CourtDto(
    Guid Id,
    string Name,
    string Slug,
    string SurfaceType,
    bool IsActive,
    int? OverrideReservationDuration,
    IEnumerable<CourtImageDto> Images);
