namespace Facilities.Shared.DTOs;

public record OpeningHoursAvailabilityInfo(DayOfWeek DayOfWeek, TimeSpan OpenTime, TimeSpan CloseTime);
