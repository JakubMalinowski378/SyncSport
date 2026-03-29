using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.GetFacilityById;

public sealed class GetFacilityByIdCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<GetFacilityByIdCommand, GetFacilityByIdResult?>
{
    public async Task<GetFacilityByIdResult?> Handle(GetFacilityByIdCommand request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: true,
            ct: cancellationToken);

        if (facility is null)
        {
            return null;
        }

        var mondayHours = facility.WeeklyOpeningHours.GetHoursForDay(DayOfWeek.Monday);

        return new GetFacilityByIdResult(
            facility.Id.Value,
            facility.Name,
            facility.Address,
            mondayHours.OpenTime,
            mondayHours.CloseTime);
    }
}
