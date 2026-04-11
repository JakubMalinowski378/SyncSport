using Facilities.Shared;
using MediatR;
using Pricing.Shared;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Commands.CreateReservation;

internal sealed class CreateReservationCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    IReservationChecker reservationChecker,
    IFacilitiesModuleApi facilitiesModuleApi,
    IPricingModuleApi pricingModuleApi)
    : IRequestHandler<CreateReservationCommand, Guid>
{
    public async Task<Guid> Handle(CreateReservationCommand request, CancellationToken cancellationToken)
    {
        var timeRange = TimeRange.Create(request.StartTime, request.EndTime);

        var isCourtAvailable = await reservationChecker.IsCourtAvailableAsync(request.CourtId, request.StartTime, request.EndTime, cancellationToken);
        if (!isCourtAvailable)
        {
            throw new ReservationOverlapException();
        }

        var hasConcurrent = await reservationChecker.IsUserHasConcurrentReservationAsync(request.UserId, request.StartTime, request.EndTime, cancellationToken);
        if (hasConcurrent)
        {
            throw new UserAlreadyHasReservationException();
        }

        var facilityId = await facilitiesModuleApi.GetFacilityIdByCourtIdAsync(request.CourtId, cancellationToken);
        if (facilityId is null)
        {
            throw new InvalidOperationException("Facility not found for the given court.");
        }

        var price = await pricingModuleApi.CalculatePriceAsync(facilityId.Value, request.CourtId, request.StartTime, request.EndTime, cancellationToken);

        var reservation = Reservation.Create(request.UserId, request.CourtId, timeRange, price);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
