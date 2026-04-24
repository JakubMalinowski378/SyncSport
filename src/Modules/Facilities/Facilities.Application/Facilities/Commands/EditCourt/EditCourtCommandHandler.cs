using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Exceptions;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Facilities.Application.Facilities.Commands.EditCourt;

public sealed class EditCourtCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IFacilityAuthorizationService facilityAuthorizationService) : IRequestHandler<EditCourtCommand>
{
    public async Task Handle(EditCourtCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var facilityId = new FacilityId(request.FacilityId);
        var courtId = new CourtId(request.CourtId);

        var facility = await facilityRepository.GetByIdAsync(
            facilityId,
            include: query => query.Include(f => f.Courts),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new Exception("Facility not found.");
        }

        var images = request.Images?
            .Select(x => ImageUrl.Create(x.Url, x.IsMain))
            .ToList();

        facility.EditCourt(courtId, request.Name, request.IsActive, request.OverrideReservationDuration, images);

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
    }
}
