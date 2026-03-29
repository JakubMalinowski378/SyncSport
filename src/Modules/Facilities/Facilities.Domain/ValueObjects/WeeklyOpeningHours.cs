using Shared.Domain;

namespace Facilities.Domain.ValueObjects;

public class WeeklyOpeningHours : ValueObject
{
    private readonly Dictionary<DayOfWeek, DailyOpeningHours> _hoursPerDay;

    public IReadOnlyDictionary<DayOfWeek, DailyOpeningHours> HoursPerDay => _hoursPerDay.AsReadOnly();

    private WeeklyOpeningHours(Dictionary<DayOfWeek, DailyOpeningHours> hoursPerDay)
    {
        if (hoursPerDay.Count != 7)
        {
            throw new ArgumentException("Weekly opening hours must contain all 7 days of the week.");
        }

        var allDaysPresent = Enum.GetValues<DayOfWeek>().All(day => hoursPerDay.ContainsKey(day));
        if (!allDaysPresent)
        {
            throw new ArgumentException("Weekly opening hours must contain all 7 days of the week.");
        }

        _hoursPerDay = new Dictionary<DayOfWeek, DailyOpeningHours>(hoursPerDay);
    }

    public static WeeklyOpeningHours Create(IEnumerable<DailyOpeningHours> dailyHours)
    {
        var hoursDict = dailyHours.ToDictionary(h => h.DayOfWeek, h => h);
        return new WeeklyOpeningHours(hoursDict);
    }

    public static WeeklyOpeningHours CreateUniform(TimeSpan openTime, TimeSpan closeTime)
    {
        var days = new List<DailyOpeningHours>();

        foreach (var day in Enum.GetValues<DayOfWeek>())
        {
            days.Add(DailyOpeningHours.Create(day, openTime, closeTime));
        }

        return Create(days);
    }

    public DailyOpeningHours GetHoursForDay(DayOfWeek day)
    {
        return _hoursPerDay[day];
    }

    public bool IsOpenAtDateTime(DateTime dateTime)
    {
        var dailyHours = GetHoursForDay(dateTime.DayOfWeek);
        return dailyHours.IsWithinHours(dateTime.TimeOfDay);
    }

    protected override IEnumerable<object?> GetAtomicValues()
    {
        yield return string.Join("|",
            Enum.GetValues<DayOfWeek>()
                .OrderBy(d => d)
                .Select(d => $"{d}:{_hoursPerDay[d].OpenTime}-{_hoursPerDay[d].CloseTime}-{_hoursPerDay[d].IsClosed}"));
    }
}
