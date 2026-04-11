using MediatR;

namespace Pricing.Application.Tariffs.Queries.CalculatePrice;

public record CalculatePriceQuery(
    Guid FacilityId,
    Guid? CourtId,
    DateTime StartTime,
    DateTime EndTime
) : IRequest<decimal>;