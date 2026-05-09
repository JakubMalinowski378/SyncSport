namespace Facilities.Shared.DTOs;

public record OpeningHoursAvailabilityInfo(DayOfWeek DayOfWeek, TimeOnly? OpenTime, TimeOnly? CloseTime);
