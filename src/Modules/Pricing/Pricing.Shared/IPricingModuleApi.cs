namespace Pricing.Shared;

public interface IPricingModuleApi
{
    Task<decimal> CalculatePriceAsync(Guid facilityId, Guid? courtId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default);
}