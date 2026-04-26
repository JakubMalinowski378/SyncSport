using MediatR;
using Microsoft.EntityFrameworkCore;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;
using Users.Shared.Authorization;

namespace Pricing.Application.Tariffs.Commands.EditTariff;

internal sealed class EditTariffCommandHandler(
    IRepository<Tariff, TariffId> repository,
    IFacilityAuthorizationService facilityAuthorizationService)
    : IRequestHandler<EditTariffCommand, bool>
{
    public async Task<bool> Handle(EditTariffCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var tariffs = await repository.FindAsync(
            predicate: t => t.FacilityId == request.FacilityId && t.CourtId == null,
            include: q => q.Include(t => t.CourtRateOverrides),
            asNoTracking: false,
            ct: cancellationToken);

        var tariff = tariffs.FirstOrDefault();
        if (tariff is null)
        {
            return false;
        }

        tariff.UpdateBaseRate(new Money(request.BaseHourlyRate));

        if (request.CourtOverrides is not null)
        {
            foreach (var courtOverride in request.CourtOverrides)
            {
                tariff.SetCourtRateOverride(courtOverride.CourtId, new Money(courtOverride.HourlyRate));
            }
        }

        repository.Update(tariff);
        await repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}
