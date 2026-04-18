using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed class GetFacilityCourtsQueryHandler(IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<GetFacilityCourtsQuery, PagedResult<CourtDto>>
{
    public async Task<PagedResult<CourtDto>> Handle(GetFacilityCourtsQuery request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            include: query => query.Include(f => f.Courts),
            asNoTracking: true,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new ArgumentException("Facility not found");
        }

        var courts = facility.Courts
            .OrderBy(c => c.Name)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(c => new CourtDto(
                c.Id.Value,
                c.Name,
                c.SurfaceType,
                c.IsActive))
            .ToList();

        var totalCount = facility.Courts.Count;

        return new PagedResult<CourtDto>(
            courts,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
