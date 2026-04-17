using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class DateSpecificOpeningHours : ValueObject
{
    public DateOnly Date { get; }
    public TimeSpan OpenTime { get; }
    public TimeSpan CloseTime { get; }
    public bool IsClosed { get; }

    private DateSpecificOpeningHours(DateOnly date, TimeSpan openTime, TimeSpan closeTime, bool isClosed)
    {
        Date = date;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public static DateSpecificOpeningHours Create(DateOnly date, TimeSpan openTime, TimeSpan closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        return new DateSpecificOpeningHours(date, openTime, closeTime, isClosed: false);
    }

    public static DateSpecificOpeningHours CreateClosed(DateOnly date)
    {
        return new DateSpecificOpeningHours(date, TimeSpan.Zero, TimeSpan.Zero, isClosed: true);
    }

    public bool IsWithinHours(TimeSpan time)
    {
        if (IsClosed)
            return false;

        return time >= OpenTime && time <= CloseTime;
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return Date;
        yield return OpenTime;
        yield return CloseTime;
        yield return IsClosed;
    }
}
