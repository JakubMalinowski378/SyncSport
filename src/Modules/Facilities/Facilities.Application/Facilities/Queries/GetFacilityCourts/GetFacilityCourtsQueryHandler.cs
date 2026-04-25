using Facilities.Application.Facilities.Common;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed class GetFacilityCourtsQueryHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IRepository<Court, CourtId> courtRepository) 
    : IRequestHandler<GetFacilityCourtsQuery, PagedResult<CourtDto>>
{
    public async Task<PagedResult<CourtDto>> Handle(GetFacilityCourtsQuery request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.FirstOrDefaultAsync(
            f => f.Slug == request.FacilitySlug,
            asNoTracking: true,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new ArgumentException("Facility not found");
        }

        var facilityId = facility.Id;

        var pagedResult = await courtRepository.GetPagedAsync(
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            filter: c => EF.Property<FacilityId>(c, "FacilityId") == facilityId,
            orderBy: q => q.OrderBy(c => c.Name),
            asNoTracking: true,
            ct: cancellationToken);

        var courts = pagedResult.Items.Select(c => new CourtDto(
            c.Id.Value,
            c.Name,
            c.Slug,
            c.SurfaceType,
            c.IsActive,
            c.OverrideReservationDuration,
            c.Images.Select(img => new ImageDto(img.Value, img.IsMain)).ToList())).ToList();

        return new PagedResult<CourtDto>(
            courts,
            pagedResult.TotalCount,
            request.PageNumber,
            request.PageSize);
    }
}
