using Pricing.Domain.ValueObjects;
using Shared.Domain;

namespace Pricing.Domain.Entities;

public partial class Tariff : AggregateRoot<TariffId>
{
    private readonly List<PriceRule> _priceRules = [];
    private readonly List<CourtRateOverride> _courtRateOverrides = [];

    public Guid FacilityId { get; private set; }
    public Guid? CourtId { get; private set; }
    public Money BaseHourlyRate { get; private set; } = default!;

    public IReadOnlyCollection<PriceRule> PriceRules => _priceRules.AsReadOnly();
    public IReadOnlyCollection<CourtRateOverride> CourtRateOverrides => _courtRateOverrides.AsReadOnly();
}