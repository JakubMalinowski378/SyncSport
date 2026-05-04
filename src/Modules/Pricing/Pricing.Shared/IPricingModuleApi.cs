namespace Pricing.Shared;

public interface IPricingModuleApi
{
    Task<decimal> CalculatePriceAsync(Guid facilityId, Guid? courtId, DateTimeOffset startTime, DateTimeOffset endTime, CancellationToken cancellationToken = default);
}