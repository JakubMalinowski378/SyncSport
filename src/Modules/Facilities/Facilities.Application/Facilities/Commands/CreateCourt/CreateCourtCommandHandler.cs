using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.CreateCourt;

public sealed class CreateCourtCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<CreateCourtCommand, Guid>
{
    public async Task<Guid> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new InvalidOperationException("Facility not found.");
        }

        var court = facility.AddCourt(request.Name, request.SurfaceType);

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return court.Id.Value;
    }
}
