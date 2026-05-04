namespace Facilities.Application.Facilities.Common;

public sealed record DailyHoursDto(DayOfWeek DayOfWeek, TimeOnly OpenTime, TimeOnly CloseTime, bool IsClosed);
public sealed record DateSpecificHoursDto(DateOnly Date, TimeOnly OpenTime, TimeOnly CloseTime, bool IsClosed);