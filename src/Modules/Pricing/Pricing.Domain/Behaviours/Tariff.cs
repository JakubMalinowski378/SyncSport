using Pricing.Domain.ValueObjects;
using Shared.Domain;

namespace Pricing.Domain.Entities;

public partial class Tariff : AggregateRoot<TariffId>
{
    protected Tariff() { }

    private Tariff(TariffId id, Guid facilityId, Money baseHourlyRate)
        : base(id)
    {
        FacilityId = facilityId;
        CourtId = null;
        BaseHourlyRate = baseHourlyRate;
    }

    public static Tariff Create(Guid facilityId, Money baseHourlyRate)
    {
        var tariff = new Tariff(TariffId.New(), facilityId, baseHourlyRate);

        return tariff;
    }

    public void AddRule(PriceRule rule)
    {
        _priceRules.Add(rule);
    }

    public void RemoveRule(PriceRuleId ruleId)
    {
        var rule = _priceRules.FirstOrDefault(r => r.Id == ruleId);
        if (rule is not null)
        {
            _priceRules.Remove(rule);
        }
    }

    public void UpdateBaseRate(Money newRate)
    {
        BaseHourlyRate = newRate;
    }

    public void SetCourtRateOverride(Guid courtId, Money hourlyRate)
    {
        var existingOverride = _courtRateOverrides.FirstOrDefault(x => x.CourtId == courtId);

        if (existingOverride is null)
        {
            _courtRateOverrides.Add(CourtRateOverride.Create(courtId, hourlyRate));
            return;
        }

        existingOverride.UpdateHourlyRate(hourlyRate);
    }

    public Money CalculatePrice(DateTime start, DateTime end, Guid? courtId = null)
    {
        var durationInHours = (decimal)(end - start).TotalHours;

        if (durationInHours <= 0)
        {
            throw new ArgumentException("End time must be greater than start time.");
        }

        var effectiveHourlyRate = BaseHourlyRate;

        if (courtId.HasValue)
        {
            var courtOverride = _courtRateOverrides.FirstOrDefault(x => x.CourtId == courtId.Value);
            if (courtOverride is not null)
            {
                effectiveHourlyRate = courtOverride.HourlyRate;
            }
        }

        var basePriceAmount = effectiveHourlyRate.Amount * durationInHours;
        var finalPrice = new Money(basePriceAmount);

        foreach (var rule in _priceRules)
        {
            finalPrice = rule.Apply(finalPrice, start, end);
        }

        return finalPrice;
    }
}