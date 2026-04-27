using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
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

        var court = facility.Courts.FirstOrDefault(c => c.Id.Value == request.CourtId);
        if (court is null)
        {
            throw new Exception("Court not found.");
        }

        List<ImageUrl>? images = null;
        var hasImageChanges =
            (request.RemovedImageUrls is not null && request.RemovedImageUrls.Count > 0) ||
            (request.Images is not null && request.Images.Count > 0) ||
            request.MainImageIndex.HasValue;

        if (hasImageChanges)
        {
            var currentImages = court.Images.ToList();
            var removedUrls = request.RemovedImageUrls is null
                ? []
                : request.RemovedImageUrls.ToHashSet(StringComparer.OrdinalIgnoreCase);

            var removedExistingImages = currentImages
                .Where(x => removedUrls.Contains(x.Value))
                .ToList();

            var imagesToKeep = currentImages
                .Where(x => !removedUrls.Contains(x.Value))
                .ToList();

            foreach (var removedImage in removedExistingImages)
            {
                await imageStorageService.DeleteAsync(removedImage.Value, cancellationToken);
            }

            var uploadedImageUrls = request.Images is not null && request.Images.Count > 0
                ? (await imageStorageService.AddRangeAsync(request.Images.ToUploadStreams(), cancellationToken)).ToList()
                : [];

            var finalImages = imagesToKeep
                .Concat(uploadedImageUrls.Select(url => ImageUrl.Create(url, false)))
                .ToList();

            if (request.MainImageIndex.HasValue && request.MainImageIndex.Value >= finalImages.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(request.MainImageIndex), "MainImageIndex must point to an existing image after applying image changes.");
            }

            var mainImageUrl = request.MainImageIndex.HasValue
                ? finalImages[request.MainImageIndex.Value].Value
                : finalImages.FirstOrDefault(x => x.IsMain)?.Value ?? finalImages.FirstOrDefault()?.Value;

            images = finalImages
                .Select(x => ImageUrl.Create(x.Value, x.Value == mainImageUrl))
                .ToList();
        }

        facility.EditCourt(courtId, request.Name, court.Slug, request.IsActive, request.OverrideReservationDuration, images);

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);
    }
}
