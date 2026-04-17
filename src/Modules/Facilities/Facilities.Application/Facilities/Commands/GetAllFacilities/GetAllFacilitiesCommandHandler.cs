using Facilities.Application.Facilities.Common;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Pagination;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.GetAllFacilities;

public sealed class GetAllFacilitiesCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<GetAllFacilitiesCommand, PagedResult<GetAllFacilitiesResult>>
{
    public async Task<PagedResult<GetAllFacilitiesResult>> Handle(GetAllFacilitiesCommand request, CancellationToken cancellationToken)
    {
        Func<IQueryable<Facility>, IOrderedQueryable<Facility>> orderBy = request.SortColumn?.ToLower() switch
        {
            "address" => request.SortOrder?.ToLower() == "desc" ? q => q.OrderByDescending(x => x.Address) : q => q.OrderBy(x => x.Address),
            _ => request.SortOrder?.ToLower() == "desc" ? q => q.OrderByDescending(x => x.Name) : q => q.OrderBy(x => x.Name)
        };

        var (items, totalCount) = await facilityRepository.GetPagedAsync(       
            request.PageNumber,
            request.PageSize,
            filter: x =>
                string.IsNullOrWhiteSpace(request.SearchTerm) || 
                x.Name.Contains(request.SearchTerm) || 
                x.Address.Contains(request.SearchTerm),
            orderBy: orderBy,
            asNoTracking: true,
            ct: cancellationToken);

        var mappedItems = items
            .Select(facility => new GetAllFacilitiesResult(
                facility.Id.Value,
                facility.Name,
                facility.Address,
                OpeningHoursMapper.MapToDto(facility.WeeklyOpeningHours)))      
            .ToList();

        return new PagedResult<GetAllFacilitiesResult>(
            mappedItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
