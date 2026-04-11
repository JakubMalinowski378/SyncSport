using MediatR;

namespace Pricing.Application.Tariffs.Commands.CreateTariff;

public record CreateTariffCommand(
    Guid FacilityId,
    Guid? CourtId,
    decimal BaseHourlyRate
) : IRequest<Guid>;