using MediatR;

namespace Pricing.Application.Tariffs.Queries.CalculatePrice;

public record CalculatePriceQuery(
    Guid FacilityId,
    Guid? CourtId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime
) : IRequest<decimal>;