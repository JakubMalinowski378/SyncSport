using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Facilities.Shared;
using Facilities.Shared.DTOs;
using Shared.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Facilities.Infrastructure.Services;

internal sealed class FacilitiesModuleApi(
    IRepository<Facility, FacilityId> facilityRepository,
    IRepository<Court, CourtId> courtRepository)
    : IFacilitiesModuleApi
{
    public async Task<FacilityAvailabilityDto?> GetFacilityAvailabilityInfoAsync(Guid facilityId, CancellationToken cancellationToken = default)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(facilityId),
            include: q => q.Include(f => f.Courts),
            asNoTracking: true,
            ct: cancellationToken);

        if (facility is null)
            return null;

        var courts = facility.Courts.Select(c => new CourtAvailabilityInfo(
            c.Id.Value,
            c.Name,
            c.OverrideReservationDuration ?? facility.ReservationDuration));

        var openingHours = facility.WeeklyOpeningHours.HoursPerDay
            .Where(x => !x.Value.IsClosed)
            .Select(x => new OpeningHoursAvailabilityInfo(x.Key, x.Value.OpenTime, x.Value.CloseTime));

        return new FacilityAvailabilityDto(facilityId, facility.Name, courts, openingHours);
    }

    public async Task<Guid?> GetFacilityIdByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default)
    {
        var courtIdVo = new CourtId(courtId);

        var facility = await facilityRepository.FirstOrDefaultAsync(
            predicate: f => f.Courts.Any(c => c.Id == courtIdVo),
            asNoTracking: true,
            ct: cancellationToken);

        return facility?.Id.Value;
    }

    public async Task<CourtDto?> GetCourtByIdAsync(Guid courtId, CancellationToken cancellationToken = default)
    {
        var courtIdVo = new CourtId(courtId);

        var court = await courtRepository.GetByIdAsync(
            courtIdVo,
            asNoTracking: true,
            ct: cancellationToken);

        if (court is null)
            return null;

        return new CourtDto(
            court.Id.Value,
            court.Name,
            court.Slug,
            court.SurfaceType,
            court.IsActive,
            court.OverrideReservationDuration,
            court.Images.Select(i => new CourtImageDto(i.Value)));
    }

    public async Task<IReadOnlyDictionary<Guid, CourtWithFacilityDto>> GetCourtsWithFacilityByIdsAsync(
        IEnumerable<Guid> courtIds,
        CancellationToken cancellationToken = default)
    {
        var courtIdValues = courtIds.ToHashSet();
        if (courtIdValues.Count == 0)
            return new Dictionary<Guid, CourtWithFacilityDto>();

        var courtIdVos = courtIdValues.Select(id => new CourtId(id)).ToHashSet();

        var facilities = await facilityRepository.FindAsync(
            predicate: f => f.Courts.Any(c => courtIdVos.Contains(c.Id)),
            include: q => q.Include(f => f.Courts),
            asNoTracking: true,
            ct: cancellationToken);

        return facilities
            .SelectMany(f => f.Courts
                .Where(c => courtIdValues.Contains(c.Id.Value))
                .Select(c => new CourtWithFacilityDto(
                    c.Id.Value,
                    c.Name,
                    f.Id.Value,
                    f.Name)))
            .ToDictionary(dto => dto.CourtId);
    }
}