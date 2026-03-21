using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class OpeningHours : ValueObject
{
    public TimeSpan OpenTime { get; }
    public TimeSpan CloseTime { get; }

    public OpeningHours(TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        OpenTime = openTime;
        CloseTime = closeTime;
    }

    public static OpeningHours Create(TimeSpan openTime, TimeSpan closeTime)
    {
        return new OpeningHours(openTime, closeTime);
    }

    public bool IsWithinHours(TimeSpan time)
    {
        return time >= OpenTime && time <= CloseTime;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return OpenTime;
        yield return CloseTime;
    }
}
