using Users.Shared.Authorization;
using Facilities.Application.Facilities.Common;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Extensions;
using Shared.Persistence;
using Storage;

namespace Facilities.Application.Facilities.Commands.CreateCourt;

public sealed class CreateCourtCommandHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IRepository<Court, CourtId> courtRepository,
    IFacilityAuthorizationService facilityAuthorizationService,
    IImageStorageService imageStorageService) : IRequestHandler<CreateCourtCommand, Guid>
{
    public async Task<Guid> Handle(CreateCourtCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);
        
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(request.FacilityId),
            asNoTracking: false,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new InvalidOperationException("Facility not found.");
        }

        var slug = await UniqueSlugProvider.GenerateAsync(
            request.Name,
            async candidate => await courtRepository.AnyAsync(c => c.Slug == candidate, cancellationToken));

        var court = facility.AddCourt(request.Name, slug, request.SurfaceType, request.OverrideReservationDuration);

        if (request.Images is not null && request.Images.Count > 0)
        {
            var imageUrls = (await imageStorageService.AddRangeAsync(request.Images.ToUploadStreams(), cancellationToken)).ToList();
            var selectedMainIndex = request.MainImageIndex ?? 0;

            for (var index = 0; index < imageUrls.Count; index++)
            {
                court.AddImage(ImageUrl.Create(imageUrls[index], index == selectedMainIndex));
            }
        }

        facilityRepository.Update(facility);
        await facilityRepository.SaveChangesAsync(cancellationToken);

        return court.Id.Value;
    }
}
