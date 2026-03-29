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
    private static readonly int[] AllowedPageSizes = [5, 10, 15, 20, 25, 30];

    public async Task<PagedResult<GetAllFacilitiesResult>> Handle(GetAllFacilitiesCommand request, CancellationToken cancellationToken)
    {
        if (request.PageNumber < 1)
        {
            throw new ArgumentException("PageNumber must be greater than 0.");
        }

        if (!AllowedPageSizes.Contains(request.PageSize))
        {
            throw new ArgumentException("PageSize must be one of: 5, 10, 15, 20, 25, 30.");
        }

        var (items, totalCount) = await facilityRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            orderBy: query => query.OrderBy(x => x.Name),
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
