using Facilities.Shared.Events;
using MediatR;
using Microsoft.Extensions.Logging;
using Pricing.Domain.Entities;
using Pricing.Domain.ValueObjects;
using Shared.Persistence;

namespace Pricing.Application.Handlers;

internal sealed class FacilityCreatedEventHandler(
    IRepository<Tariff, TariffId> repository,
    ILogger<FacilityCreatedEventHandler> logger) : INotificationHandler<FacilityCreatedEvent>
{
    public async Task Handle(FacilityCreatedEvent notification, CancellationToken cancellationToken)
    {
        logger.LogInformation("Handling FacilityCreatedEvent for FacilityId: {FacilityId}", notification.FacilityId);

        var tariffs = await repository.FindAsync(
            predicate: t => t.FacilityId == notification.FacilityId,
            asNoTracking: false,
            ct: cancellationToken);

        if (tariffs.Any())
        {
            logger.LogInformation("Tariff already exists for FacilityId: {FacilityId}", notification.FacilityId);
            return;
        }

        var tariff = Tariff.Create(notification.FacilityId, new Money(30m));

        await repository.AddAsync(tariff, cancellationToken);
        await repository.SaveChangesAsync(cancellationToken);

        logger.LogInformation("Created default tariff with rate 30 for FacilityId: {FacilityId}", notification.FacilityId);
    }
}