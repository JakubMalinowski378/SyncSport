using Shared.Domain;

namespace Reservations.Domain.ValueObjects;

public sealed class TimeRange : ValueObject
{
    public DateTimeOffset Start { get; }
    public DateTimeOffset End { get; }

    private TimeRange(DateTimeOffset start, DateTimeOffset end)
    {
        Start = start;
        End = end;
    }

    public static TimeRange Create(DateTimeOffset start, DateTimeOffset end)
    {
        if (start >= end)
        {
            throw new ArgumentException("Start time must be before end time.");
        }

        var duration = end - start;
        if (duration.TotalMinutes < 60 || duration.TotalMinutes > 120)
        {
            throw new ArgumentException("Reservation duration must be between 60 and 120 minutes.");
        }

        return new TimeRange(start, end);
    }

    public bool Overlaps(TimeRange other)
    {
        return Start < other.End && End > other.Start;
    }

    protected override IEnumerable<object> GetAtomicValues()
    {
        yield return Start;
        yield return End;
    }
}
