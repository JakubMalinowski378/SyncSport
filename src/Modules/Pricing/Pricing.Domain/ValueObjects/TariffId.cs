using Shared.Domain;

namespace Pricing.Domain.ValueObjects;

public sealed class TariffId : ValueObject
{
    public Guid Value { get; }

    public TariffId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("Id cannot be empty");
        }

        Value = value;
    }

    public static TariffId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}