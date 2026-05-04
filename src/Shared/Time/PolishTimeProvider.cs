namespace Shared.Time;

public static class PolishTimeProvider
{
    public static readonly TimeZoneInfo PolishTimeZone;

    static PolishTimeProvider()
    {
        try
        {
            PolishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Europe/Warsaw");
        }
        catch (TimeZoneNotFoundException)
        {
            PolishTimeZone = TimeZoneInfo.FindSystemTimeZoneById("Central European Standard Time");
        }
    }

    public static DateTimeOffset ConvertPolishLocalToUtc(DateTimeOffset polishLocal)
    {
        if (polishLocal.Offset == TimeSpan.Zero)
            return polishLocal;

        return TimeZoneInfo.ConvertTime(polishLocal, PolishTimeZone);
    }

    public static DateTimeOffset ConvertUtcToPolishLocal(DateTimeOffset utc)
    {
        return TimeZoneInfo.ConvertTime(utc, PolishTimeZone);
    }

    public static DateTimeOffset PolishMidnightToUtc(DateOnly date)
    {
        var polishMidnight = date.ToDateTime(TimeOnly.MinValue);
        var polishDto = new DateTimeOffset(polishMidnight, PolishTimeZone.GetUtcOffset(polishMidnight));
        return ConvertPolishLocalToUtc(polishDto);
    }
}
