using Pricing.Domain.Enums;
using Pricing.Domain.ValueObjects;
using Shared.Domain;

namespace Pricing.Domain.Entities;

public partial class PriceRule : Entity<PriceRuleId>
{
    public RuleType Type { get; private set; }
    public DayOfWeek? DayOfWeek { get; private set; }
    public TimeOnly? StartTime { get; private set; }
    public TimeOnly? EndTime { get; private set; }
    public decimal Multiplier { get; private set; }

    public TariffId TariffId { get; private set; } = default!;
}