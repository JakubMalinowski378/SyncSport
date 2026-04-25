using Facilities.Application.Facilities.Common;
using MediatR;
using Shared.Pagination;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed record GetFacilityCourtsQuery(
    string FacilitySlug = "",
    int PageNumber = 1,
    int PageSize = 10) : IRequest<PagedResult<CourtDto>>;

public sealed record CourtDto(
    Guid Id,
    string Name,
    string Slug,
    string SurfaceType,
    bool IsActive,
    int? OverrideReservationDuration,
    List<ImageDto> Images);

