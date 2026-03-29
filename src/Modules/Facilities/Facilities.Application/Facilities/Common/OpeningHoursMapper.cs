using Facilities.Domain.ValueObjects;
using System.Globalization;

namespace Facilities.Application.Facilities.Common;

public static class OpeningHoursMapper
{
    public static List<DailyOpeningHoursDto> MapToDto(WeeklyOpeningHours weeklyOpeningHours)
    {
        var culture = new CultureInfo("en-US");

        return weeklyOpeningHours.HoursPerDay.Values
            .OrderBy(h => h.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)h.DayOfWeek)
            .Select(h => new DailyOpeningHoursDto(
                DayName: culture.DateTimeFormat.GetDayName(h.DayOfWeek),
                OpenTime: h.OpenTime,
                CloseTime: h.CloseTime,
                IsClosed: h.IsClosed))
            .ToList();
    }
}