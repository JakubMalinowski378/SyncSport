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

    public static DateTime ConvertPolishLocalToUtc(DateTime polishLocal)
    {
        if (polishLocal.Kind == DateTimeKind.Utc)
            return polishLocal;

        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(polishLocal, DateTimeKind.Unspecified),
            PolishTimeZone);
    }

    public static DateTime ConvertUtcToPolishLocal(DateTime utc)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            PolishTimeZone);
    }

    public static DateTime PolishMidnightToUtc(DateOnly date)
    {
        var polishMidnight = date.ToDateTime(TimeOnly.MinValue);
        return ConvertPolishLocalToUtc(polishMidnight);
    }
}
