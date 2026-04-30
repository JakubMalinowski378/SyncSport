using Facilities.Shared.DTOs;
namespace Facilities.Shared;

public interface IFacilitiesModuleApi
{
    Task<Guid?> GetFacilityIdByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default);
    Task<FacilityAvailabilityDto?> GetFacilityAvailabilityInfoAsync(Guid facilityId, CancellationToken cancellationToken = default);
    Task<CourtDto?> GetCourtByIdAsync(Guid courtId, CancellationToken cancellationToken = default);
}