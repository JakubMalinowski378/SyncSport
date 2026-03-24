using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.CreateFacility;

public sealed class CreateFacilityCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository) : IRequestHandler<CreateFacilityCommand, Guid>
{
    public async Task<Guid> Handle(CreateFacilityCommand request, CancellationToken cancellationToken)
    {
        var existingFacility = await facilityRepository.FirstOrDefaultAsync(
            x => x.Name == request.Name,
            asNoTracking: true,
            ct: cancellationToken);

        if (existingFacility is not null)
        {
            throw new InvalidOperationException("A facility with this name already exists.");
        }

        var openingHours = OpeningHours.Create(request.OpenTime, request.CloseTime);
        var facility = Facility.Create(request.Name, request.Address, openingHours);

        await facilityRepository.AddAsync(facility, cancellationToken);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return facility.Id.Value;
    }
}
