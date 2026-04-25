using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Exceptions;
using Shared.Extensions;
using Shared.Persistence;
using Storage;
using Users.Shared.Authorization;

namespace Facilities.Application.Facilities.Commands.EditCourt;

public sealed class EditCourtCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IFacilityAuthorizationService facilityAuthorizationService,
    IImageStorageService imageStorageService) : IRequestHandler<EditCourtCommand>
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

        List<ImageUrl>? images = null;
        if (request.Images is not null && request.Images.Count > 0)
        {
            var imageUrls = (await imageStorageService.AddRangeAsync(request.Images.ToUploadStreams(), cancellationToken)).ToList();
            var selectedMainIndex = request.MainImageIndex ?? 0;
            images = new List<ImageUrl>(imageUrls.Count);

            for (var index = 0; index < imageUrls.Count; index++)
            {
                images.Add(ImageUrl.Create(imageUrls[index], index == selectedMainIndex));
            }
        }

        facility.EditCourt(courtId, request.Name, request.IsActive, request.OverrideReservationDuration, images);

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
    }
}
