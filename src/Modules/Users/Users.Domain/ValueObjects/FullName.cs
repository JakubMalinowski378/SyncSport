using Shared.Domain;

namespace Users.Domain.ValueObjects;

public class FullName : ValueObject
{
    public string FirstName { get; }
    public string LastName { get; }

    private FullName() { }

    private FullName(string firstName, string lastName)
    {
        FirstName = firstName;
        LastName = lastName;
    }

    public static FullName Create(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            throw new ArgumentException("First name cannot be empty.", nameof(firstName));
        
        if (string.IsNullOrWhiteSpace(lastName))
            throw new ArgumentException("Last name cannot be empty.", nameof(lastName));

        return new FullName(firstName, lastName);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return FirstName;
        yield return LastName;
    }

    public override string ToString() => $"{FirstName} {LastName}".Trim();
}
