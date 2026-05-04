using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class OpeningHours : ValueObject
{
    public TimeOnly OpenTime { get; }
    public TimeOnly CloseTime { get; }

    public OpeningHours(TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public static OpeningHours Create(TimeOnly openTime, TimeOnly closeTime)
    {
        return new OpeningHours(openTime, closeTime);
    }

    public bool IsWithinHours(TimeOnly time)
    {
        return time >= OpenTime && time <= CloseTime;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return OpenTime;
        yield return CloseTime;
    }
}
