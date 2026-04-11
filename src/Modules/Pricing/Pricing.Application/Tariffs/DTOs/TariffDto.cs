namespace Pricing.Application.Tariffs.DTOs;

public record TariffDto(
    Guid Id,
    Guid FacilityId,
    Guid? CourtId,
    decimal BaseHourlyRate,
    IEnumerable<PriceRuleDto> PriceRules
);

public record PriceRuleDto(
    Guid Id,
    string Type,
    decimal Multiplier,
    int? DayOfWeek,
    TimeSpan? StartTime,
    TimeSpan? EndTime
);