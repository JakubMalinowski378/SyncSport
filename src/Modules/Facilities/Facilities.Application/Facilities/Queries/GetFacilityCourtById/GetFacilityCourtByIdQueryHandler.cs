using Facilities.Application.Facilities.Common;
using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourtById;

public sealed class GetFacilityCourtByIdQueryHandler(
    IRepository<Facility, FacilityId> facilityRepository,
    IRepository<Court, CourtId> courtRepository)
    : IRequestHandler<GetFacilityCourtByIdQuery, CourtDto?>
{
    public async Task<CourtDto?> Handle(GetFacilityCourtByIdQuery request, CancellationToken cancellationToken)
    {
        var facilityId = new FacilityId(request.FacilityId);
        var courtId = new CourtId(request.CourtId);

        var facilityExists = await facilityRepository.AnyAsync(f => f.Id == facilityId, cancellationToken);
        if (!facilityExists)
        {
            throw new ArgumentException("Facility not found");
        }

        var court = await courtRepository.GetByIdAsync(courtId, asNoTracking: true, ct: cancellationToken);

        if (court is null)
        {
            return null;
        }

        return new CourtDto(
            court.Id.Value,
            court.Name,
            court.SurfaceType,
            court.IsActive,
            court.OverrideReservationDuration,
            court.Images.Select(img => new ImageDto(img.Value, img.IsMain)).ToList());
    }
}
