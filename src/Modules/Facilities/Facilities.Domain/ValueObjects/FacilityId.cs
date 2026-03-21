using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class FacilityId : ValueObject
{
    public Guid Value { get; }

    public FacilityId(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new ArgumentException("FacilityId cannot be an empty GUID");
        }
        Value = value;
    }

    public static FacilityId New() => new(Guid.NewGuid());

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }
}
