using MediatR;
using Pricing.Application.Tariffs.Queries.CalculatePrice;
using Pricing.Shared;

namespace Pricing.Infrastructure.Services;

internal sealed class PricingModuleApi(ISender sender) : IPricingModuleApi
{
    public Task<decimal> CalculatePriceAsync(Guid facilityId, Guid? courtId, DateTime startTime, DateTime endTime, CancellationToken cancellationToken = default)
    {
        return sender.Send(new CalculatePriceQuery(facilityId, courtId, startTime, endTime), cancellationToken);
    }
}