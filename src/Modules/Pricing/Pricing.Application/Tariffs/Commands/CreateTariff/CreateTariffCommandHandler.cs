using MediatR;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Pricing.Application.Tariffs.Commands.CreateTariff;

internal sealed class CreateTariffCommandHandler(
    IRepository<Tariff, TariffId> repository,
    IFacilityAuthorizationService facilityAuthorizationService) 
    : IRequestHandler<CreateTariffCommand, Guid>
{
    public async Task<Guid> Handle(CreateTariffCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var tariff = Tariff.Create(
            request.FacilityId,
            request.CourtId,
            new Money(request.BaseHourlyRate));

        await repository.AddAsync(tariff, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        return tariff.Id.Value;
    }
}