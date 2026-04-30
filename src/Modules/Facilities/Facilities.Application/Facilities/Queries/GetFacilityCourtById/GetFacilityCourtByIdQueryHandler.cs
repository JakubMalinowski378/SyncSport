using Facilities.Application.Facilities.Common;
using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Persistence;

namespace Facilities.Application.Facilities.Queries.GetFacilityCourtById;

public sealed class GetFacilityCourtByIdQueryHandler(
    IRepository<Facility, FacilityId> facilityRepository)
    : IRequestHandler<GetFacilityCourtByIdQuery, CourtDto?>
{
    public async Task<CourtDto?> Handle(GetFacilityCourtByIdQuery request, CancellationToken cancellationToken)
    {
        var facility = await facilityRepository.FirstOrDefaultAsync(
            f => f.Slug == request.FacilitySlug,
            include: q => q.Include(f => f.Courts),
            asNoTracking: true,
            ct: cancellationToken);

        if (facility is null)
        {
            throw new ArgumentException("Facility not found");
        }

        var court = facility.Courts.FirstOrDefault(c => c.Slug == request.CourtSlug);

        if (court is null)
        {
            return null;
        }

        return new CourtDto(
            court.Id.Value,
            court.Name,
            court.Slug,
            court.SurfaceType,
            court.IsActive,
            court.OverrideReservationDuration,
            court.Images.Select(img => new ImageDto(img.Value)).ToList());
    }
}
