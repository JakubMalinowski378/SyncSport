namespace Facilities.Application.Facilities.Common;

public sealed record DailyOpeningHoursDto(
    string DayName,
    TimeOnly? OpenTime,
    TimeOnly? CloseTime,
    bool IsClosed);
