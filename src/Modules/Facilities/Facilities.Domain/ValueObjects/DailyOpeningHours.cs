using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class DailyOpeningHours : ValueObject
{
    public DayOfWeek DayOfWeek { get; }
    public TimeOnly? OpenTime { get; }
    public TimeOnly? CloseTime { get; }
    public bool IsClosed { get; }

    private DailyOpeningHours(DayOfWeek dayOfWeek, TimeOnly? openTime, TimeOnly? closeTime, bool isClosed)
    {
        DayOfWeek = dayOfWeek;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public static DailyOpeningHours Create(DayOfWeek dayOfWeek, TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        return new DailyOpeningHours(dayOfWeek, openTime, closeTime, isClosed: false);
    }

    public static DailyOpeningHours CreateClosed(DayOfWeek dayOfWeek)
    {
        return new DailyOpeningHours(dayOfWeek, null, null, isClosed: true);
    }

    public bool IsWithinHours(TimeOnly time)
    {
        if (IsClosed || !OpenTime.HasValue || !CloseTime.HasValue)
            return false;

        return time >= OpenTime.Value && time <= CloseTime.Value;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return DayOfWeek;
        yield return OpenTime;
        yield return CloseTime;
        yield return IsClosed;
    }
}
