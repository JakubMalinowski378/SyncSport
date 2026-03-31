using Shared.Domain;
using System.Text.RegularExpressions;
using Users.Domain.Exceptions;

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
            throw new InvalidEmailException();
        }

        if (!EmailRegex.IsMatch(value))
        {
            throw new InvalidEmailException(value);
        }

        return new Email(value);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
