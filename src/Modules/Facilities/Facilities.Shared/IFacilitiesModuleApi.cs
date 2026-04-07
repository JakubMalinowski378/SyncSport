namespace Facilities.Shared;

public interface IFacilitiesModuleApi
{
    Task<Guid?> GetFacilityIdByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default);
}