using Facilities.Application.Facilities.Common;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetFacilityById;

public sealed class GetFacilityByIdQueryHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<GetFacilityByIdQuery, GetFacilityByIdResult?>
{
    public async Task<GetFacilityByIdResult?> Handle(GetFacilityByIdQuery request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.FirstOrDefaultAsync(
            x => x.Slug == request.FacilitySlug,
            asNoTracking: true,
            ct: cancellationToken);

        if (facility is null)
        {
            return null;
        }

        return new GetFacilityByIdResult(
            facility.Id.Value,
            facility.Name,
            facility.Slug,
            facility.Address,
            facility.ReservationDuration,
            OpeningHoursMapper.MapToDto(facility.WeeklyOpeningHours),
            facility.CustomDateHours.Select(x => new DateSpecificOpeningHoursDto(
                x.Date, x.OpenTime, x.CloseTime, x.IsClosed
            )).ToList(),
            facility.Images.Select(img => new ImageDto(img.Value)).ToList());
    }
}
