using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class DailyOpeningHours : ValueObject
{
    public DayOfWeek DayOfWeek { get; }
    public TimeSpan OpenTime { get; }
    public TimeSpan CloseTime { get; }
    public bool IsClosed { get; }

    private DailyOpeningHours(DayOfWeek dayOfWeek, TimeSpan openTime, TimeSpan closeTime, bool isClosed)
    {
        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public static DailyOpeningHours Create(DayOfWeek dayOfWeek, TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        return new DailyOpeningHours(dayOfWeek, openTime, closeTime, isClosed: false);
    }

    public bool IsWithinHours(TimeSpan time)
    {
        if (IsClosed)
            return false;

        return time >= OpenTime && time <= CloseTime;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return DayOfWeek;
        yield return OpenTime;
        yield return CloseTime;
        yield return IsClosed;
    }
}
