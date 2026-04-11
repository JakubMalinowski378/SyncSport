using MediatR;
using Pricing.Application.Tariffs.DTOs;

namespace Pricing.Application.Tariffs.Queries.GetFacilityTariffs;

public record GetFacilityTariffsQuery(Guid FacilityId) : IRequest<IEnumerable<TariffDto>>;