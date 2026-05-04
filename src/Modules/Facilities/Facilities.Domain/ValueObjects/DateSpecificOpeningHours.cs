using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class DateSpecificOpeningHours : ValueObject
{
    public DateOnly Date { get; }
    public TimeOnly OpenTime { get; }
    public TimeOnly CloseTime { get; }
    public bool IsClosed { get; }

    private DateSpecificOpeningHours(DateOnly date, TimeOnly openTime, TimeOnly closeTime, bool isClosed)
    {
        Date = date;
        OpenTime = openTime;
        CloseTime = closeTime;
        IsClosed = isClosed;
    }

    public static DateSpecificOpeningHours Create(DateOnly date, TimeOnly openTime, TimeOnly closeTime)
    {
        if (openTime >= closeTime)
        {
            throw new ArgumentException("Open time must be before close time.");
        }

        return new DateSpecificOpeningHours(date, openTime, closeTime, isClosed: false);
    }

    public static DateSpecificOpeningHours CreateClosed(DateOnly date)
    {
        return new DateSpecificOpeningHours(date, TimeOnly.MinValue, TimeOnly.MinValue, isClosed: true);
    }

    public bool IsWithinHours(TimeOnly time)
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
