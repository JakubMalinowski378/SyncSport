namespace Facilities.Application.Facilities.Common;

public sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);
public sealed record DateSpecificHoursDto(DateOnly Date, TimeSpan OpenTime, TimeSpan CloseTime, bool IsClosed);