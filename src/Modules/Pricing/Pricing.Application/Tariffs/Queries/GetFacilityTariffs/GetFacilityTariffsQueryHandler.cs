using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Application.Tariffs.DTOs;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;

namespace Pricing.Application.Tariffs.Queries.GetFacilityTariffs;

internal sealed class GetFacilityTariffsQueryHandler(
    IRepository<Tariff, TariffId> repository)
    : IRequestHandler<GetFacilityTariffsQuery, IEnumerable<TariffDto>>
{
    public async Task<IEnumerable<TariffDto>> Handle(GetFacilityTariffsQuery request, CancellationToken cancellationToken)
    {
        var tariffs = await repository.FindAsync(
            predicate: t => t.FacilityId == request.FacilityId && t.CourtId == null,
            include: q => q
                .Include(t => t.PriceRules)
                .Include(t => t.CourtRateOverrides),
            asNoTracking: true,
            ct: cancellationToken);

        return tariffs.Select(t => new TariffDto(
            t.Id.Value,
            t.FacilityId,
            t.BaseHourlyRate.Amount,
            t.CourtRateOverrides.Select(co => new CourtRateOverrideDto(
                co.CourtId,
                co.HourlyRate.Amount
            )),
            t.PriceRules.Select(pr => new PriceRuleDto(
                pr.Id.Value,
                pr.Type.ToString(),
                pr.Multiplier,
                pr.DayOfWeek.HasValue ? (int)pr.DayOfWeek.Value : null,
                pr.StartTime,
                pr.EndTime
            ))
        ));
    }
}