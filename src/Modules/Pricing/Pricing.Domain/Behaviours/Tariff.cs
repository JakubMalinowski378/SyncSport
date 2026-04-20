using Pricing.Domain.ValueObjects;
using Shared.Domain;

namespace Pricing.Domain.Entities;

public partial class Tariff : AggregateRoot<TariffId>
{
    protected Tariff() { }

    private Tariff(TariffId id, Guid facilityId, Guid? courtId, Money baseHourlyRate)
        : base(id)
    {
        FacilityId = facilityId;
        CourtId = courtId;
        BaseHourlyRate = baseHourlyRate;
    }

    public static Tariff Create(Guid facilityId, Guid? courtId, Money baseHourlyRate)
    {
        var tariff = new Tariff(TariffId.New(), facilityId, courtId, baseHourlyRate);

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

    public Money CalculatePrice(DateTime start, DateTime end)
    {
        var durationInHours = (decimal)(end - start).TotalHours;

        if (durationInHours <= 0)
        {
            throw new ArgumentException("End time must be greater than start time.");
        }

        var basePriceAmount = BaseHourlyRate.Amount * durationInHours;
        var finalPrice = new Money(basePriceAmount);

        foreach (var rule in _priceRules)
        {
            finalPrice = rule.Apply(finalPrice, start, end);
        }

        return finalPrice;
    }
}