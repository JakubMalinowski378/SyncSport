namespace Pricing.Application.Tariffs.DTOs;

public record TariffDto(
    Guid Id,
    Guid FacilityId,
    decimal BaseHourlyRate,
    IEnumerable<CourtRateOverrideDto> CourtOverrides,
    IEnumerable<PriceRuleDto> PriceRules
);

public record CourtRateOverrideDto(
    Guid CourtId,
    decimal HourlyRate
);

public record PriceRuleDto(
    Guid Id,
    string Type,
    decimal Multiplier,
    int? DayOfWeek,
    TimeOnly? StartTime,
    TimeOnly? EndTime
);