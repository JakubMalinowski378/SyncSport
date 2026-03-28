using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.RemoveFacility;

public sealed class RemoveFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<RemoveFacilityCommand, bool>
{
    public async Task<bool> Handle(RemoveFacilityCommand request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            return false;
        }

        facilityRepository.Remove(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
