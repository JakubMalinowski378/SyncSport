using Facilities.Application.Facilities.Common; using MediatR;

namespace Facilities.Application.Facilities.Queries.GetFacilityById;

public sealed record GetFacilityByIdQuery(string FacilitySlug) : IRequest<GetFacilityByIdResult?>;

public sealed record GetFacilityByIdResult(
    Guid Id,
    string Name,
    string Slug,
    string Address,
    int ReservationDuration,
    List<DailyOpeningHoursDto> OpeningHours,
    List<DateSpecificOpeningHoursDto> CustomDateHours,
    List<ImageDto> Images);
