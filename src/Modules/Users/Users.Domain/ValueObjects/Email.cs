using Shared.Domain;
using System.Text.RegularExpressions;

namespace Users.Domain.ValueObjects;

public class Email : ValueObject
{
    private static readonly Regex EmailRegex = new(
        @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
        RegexOptions.Compiled);

    public string Value { get; }

    private Email() { }

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public static Email Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Email address cannot be empty.", nameof(value));
        }

        if (!EmailRegex.IsMatch(value))
        {
            throw new ArgumentException("Invalid email format.", nameof(value));
        }

        return new Email(value);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
