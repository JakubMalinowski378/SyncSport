namespace Facilities.Application.Facilities.Common;

public sealed record DailyOpeningHoursDto(
    string DayName,
    TimeSpan OpenTime,
    TimeSpan CloseTime,
    bool IsClosed);
