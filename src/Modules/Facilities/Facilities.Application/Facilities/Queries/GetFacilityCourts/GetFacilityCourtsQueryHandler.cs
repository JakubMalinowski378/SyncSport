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
        var facilityId = new FacilityId(request.FacilityId);

        var facilityExists = await facilityRepository.AnyAsync(f => f.Id == facilityId, cancellationToken);
        if (!facilityExists)
        {
            throw new ArgumentException("Facility not found");
        }

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
