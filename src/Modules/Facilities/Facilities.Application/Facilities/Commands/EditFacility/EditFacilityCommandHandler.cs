using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.EditFacility;

public sealed class EditFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<EditFacilityCommand, bool>
{
    public async Task<bool> Handle(EditFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            return false;
        }

        var existingFacility = await facilityRepository.FirstOrDefaultAsync(
            x => x.Name == request.Name,
            asNoTracking: true,
            ct: cancellationToken);

        if (existingFacility is not null && existingFacility.Id.Value != request.FacilityId)
        {
            throw new InvalidOperationException("A facility with this name already exists.");
        }

        var weeklyOpeningHours = WeeklyOpeningHours.CreateUniform(request.OpenTime, request.CloseTime);

        facility.Rename(request.Name);
        facility.ChangeAddress(request.Address);
        facility.ChangeOpeningHours(weeklyOpeningHours);

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
