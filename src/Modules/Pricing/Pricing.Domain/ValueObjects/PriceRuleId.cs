using Shared.Domain;

namespace Pricing.Domain.ValueObjects;

public sealed class PriceRuleId : ValueObject
{
    public Guid Value { get; }

    public PriceRuleId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty");
        }

        Value = value;
    }

    public static PriceRuleId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}