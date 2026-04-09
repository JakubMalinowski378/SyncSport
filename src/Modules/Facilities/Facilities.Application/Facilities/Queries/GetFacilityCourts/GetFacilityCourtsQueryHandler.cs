using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Pagination;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourts;

public sealed class GetFacilityCourtsQueryHandler : IRequestHandler<GetFacilityCourtsQuery, PagedResult<CourtDto>>
{
    private readonly IRepository<Facility, FacilityId> _facilityRepository;
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];

    public GetFacilityCourtsQueryHandler(IRepository<Facility, FacilityId> facilityRepository)
    {
        _facilityRepository = facilityRepository;
    }

    public async Task<PagedResult<CourtDto>> Handle(GetFacilityCourtsQuery request, CancellationToken cancellationToken)
    {
        if (request.PageNumber < 1)
        {
            throw new ArgumentException("PageNumber must be greater than 0.");
        }

        if (!AllowedPageSizes.Contains(request.PageSize))
        {
            throw new ArgumentException("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
        }

        var facility = await _facilityRepository.GetByIdAsync(
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
