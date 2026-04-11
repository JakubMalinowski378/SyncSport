using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;

namespace Pricing.Application.Tariffs.Queries.CalculatePrice;

internal sealed class CalculatePriceQueryHandler(IRepository<Tariff, TariffId> repository)
    : IRequestHandler<CalculatePriceQuery, decimal>
{
    public async Task<decimal> Handle(CalculatePriceQuery request, CancellationToken cancellationToken)
    {
        var courtTariffs = await repository.FindAsync(
            predicate: t => t.FacilityId == request.FacilityId && t.CourtId == request.CourtId,
            include: q => q.Include(t => t.PriceRules),
            asNoTracking: true,
            ct: cancellationToken);

        var tariff = courtTariffs.FirstOrDefault();

        if (tariff is null)
        {
            var facilityTariffs = await repository.FindAsync(
                predicate: t => t.FacilityId == request.FacilityId && t.CourtId == null,
                include: q => q.Include(t => t.PriceRules),
                asNoTracking: true,
                ct: cancellationToken);
            
            tariff = facilityTariffs.FirstOrDefault();
        }

        if (tariff is null)
        {
            throw new Exception($"No tariff found for facility {request.FacilityId}");
        }

        var price = tariff.CalculatePrice(request.StartTime, request.EndTime);

        return price.Amount;
    }
}