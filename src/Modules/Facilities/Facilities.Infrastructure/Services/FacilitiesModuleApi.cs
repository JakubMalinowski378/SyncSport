using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Facilities.Shared;
using Shared.Persistence;

namespace Facilities.Infrastructure.Services;

internal sealed class FacilitiesModuleApi(IRepository<Facility, FacilityId> facilityRepository)
    : IFacilitiesModuleApi
{
    public async Task<Guid?> GetFacilityIdByCourtIdAsync(Guid courtId, CancellationToken cancellationToken = default)
    {
        var courtIdVo = new CourtId(courtId);

        var facility = await facilityRepository.FirstOrDefaultAsync(
            predicate: f => f.Courts.Any(c => c.Id == courtIdVo),
            asNoTracking: true,
            ct: cancellationToken);

        return facility?.Id.Value;
    }
}