using Facilities.Application.Facilities.Common;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Pagination;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetAllFacilities;

public sealed class GetAllFacilitiesCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<GetAllFacilitiesCommand, PagedResult<GetAllFacilitiesResult>>
{
    public async Task<PagedResult<GetAllFacilitiesResult>> Handle(GetAllFacilitiesCommand request, CancellationToken cancellationToken)
    {
        var managedFacilityIds = ParseManagedFacilityIds(request.ManagedFacilityIds);
        var facilityIds = managedFacilityIds.Select(id => new FacilityId(id)).ToList();

        Func<IQueryable<Facility>, IOrderedQueryable<Facility>> orderBy = request.SortColumn?.ToLower() switch
        {
            "address" => request.SortOrder?.ToLower() == "desc" ? q => q.OrderByDescending(x => x.Address) : q => q.OrderBy(x => x.Address),
            "slug" => request.SortOrder?.ToLower() == "desc" ? q => q.OrderByDescending(x => x.Slug) : q => q.OrderBy(x => x.Slug),
            _ => request.SortOrder?.ToLower() == "desc" ? q => q.OrderByDescending(x => x.Name) : q => q.OrderBy(x => x.Name)
        };

        var (items, totalCount) = await facilityRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            filter: x =>
                (facilityIds.Count == 0 || facilityIds.Contains(x.Id)) &&
                (string.IsNullOrWhiteSpace(request.SearchTerm) ||
                x.Name.Contains(request.SearchTerm) ||
                x.Slug.Contains(request.SearchTerm) ||
                x.Address.Contains(request.SearchTerm)),
            orderBy: orderBy,
            asNoTracking: true,
            ct: cancellationToken);

        var mappedItems = items
            .Select(facility => new GetAllFacilitiesResult(
                facility.Id.Value,
                facility.Name,
                facility.Slug,
                facility.Address,
                facility.ReservationDuration,
                OpeningHoursMapper.MapToDto(facility.WeeklyOpeningHours),
                facility.Images.Select(img => new ImageDto(img.Value)).ToList()))
            .ToList();

        return new PagedResult<GetAllFacilitiesResult>(
            mappedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }

    private static List<Guid> ParseManagedFacilityIds(string? managedFacilityIds)
    {
        if (string.IsNullOrWhiteSpace(managedFacilityIds))
            return [];

        return managedFacilityIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(id => Guid.TryParse(id, out var guid) ? guid : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToList();
    }
}
