using Facilities.Application.Abstractions;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.RemoveFacility;

public sealed class RemoveFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IFacilityAuthorizationService facilityAuthorizationService) : IRequestHandler<RemoveFacilityCommand>
{
    public async Task Handle(RemoveFacilityCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new Exception("Facility not found.");
        }

        facilityRepository.Remove(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
    }
}
