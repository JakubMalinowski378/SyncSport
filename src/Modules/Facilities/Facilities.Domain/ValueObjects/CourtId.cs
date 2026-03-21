using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class CourtId : ValueObject
{
    public Guid Value { get; }

    public CourtId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("CourtId cannot be an empty GUID");
        }
        Value = value;
    }

    public static CourtId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
