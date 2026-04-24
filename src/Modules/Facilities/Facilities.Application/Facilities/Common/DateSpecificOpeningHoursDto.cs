namespace Facilities.Application.Facilities.Common;

public sealed record DateSpecificOpeningHoursDto(
    DateOnly Date,
    TimeSpan OpenTime,
    TimeSpan CloseTime,
    bool IsClosed);
