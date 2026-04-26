using System.Text.Json.Serialization;
using Facilities.Application.Facilities.Common;
using MediatR;
using Shared.Pagination;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed class GetFacilityCourtsQuery : IRequest<PagedResult<CourtDto>>
{
    [JsonIgnore]
    public string FacilitySlug { get; set; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}

public sealed record CourtDto(
    Guid Id,
    string Name,
    string Slug,
    string SurfaceType,
    bool IsActive,
    int? OverrideReservationDuration,
    List<ImageDto> Images);

