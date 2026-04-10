using Shared.Domain;

namespace Pricing.Domain.ValueObjects;

public sealed class Money : ValueObject
{
    public decimal Amount { get; }

    public Money(decimal amount)
    {
        if (amount < 0)
        {
            throw new ArgumentException("Amount cannot be negative", nameof(amount));
        }

        Amount = amount;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Amount;
    }

    public static Money Zero() => new(0);

    public Money Add(Money money)
    {
        return new Money(Amount + money.Amount);
    }

    public Money Multiply(decimal multiplier) => new(Amount * multiplier);
    public static Money operator +(Money left, Money right) => left.Add(right); 
    public static Money operator *(Money val, decimal multiplier) => val.Multiply(multiplier);}