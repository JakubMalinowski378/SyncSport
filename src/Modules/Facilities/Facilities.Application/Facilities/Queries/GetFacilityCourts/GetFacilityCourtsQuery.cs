using MediatR;
using Shared.Pagination;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed record GetFacilityCourtsQuery(
    Guid FacilityId,
    int PageNumber,
    int PageSize) : IRequest<PagedResult<CourtDto>>;

public sealed record CourtDto(
    Guid Id,
    string Name,
    string SurfaceType,
    bool IsActive);
