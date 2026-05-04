namespace Facilities.Application.Facilities.Common;

public sealed record DateSpecificOpeningHoursDto(
    DateOnly Date,
    TimeOnly OpenTime,
    TimeOnly CloseTime,
    bool IsClosed);
