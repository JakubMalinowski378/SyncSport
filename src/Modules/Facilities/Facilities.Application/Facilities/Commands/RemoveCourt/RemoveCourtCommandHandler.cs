using Facilities.Application.Abstractions;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Commands.RemoveCourt;

public sealed class RemoveCourtCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IFacilityAuthorizationService facilityAuthorizationService) : IRequestHandler<RemoveCourtCommand, bool>
{
    public async Task<bool> Handle(RemoveCourtCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            include: query => query.Include(f => f.Courts),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            return false;
        }

        var court = facility.Courts.FirstOrDefault(c => c.Id.Value == request.CourtId);
        if (court is null)
        {
            return false;
        }

        facility.RemoveCourt(court.Id);
        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
