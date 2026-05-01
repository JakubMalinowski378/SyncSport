using Facilities.Shared;
using MediatR;
using Pricing.Shared;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Reservations.Domain.ValueObjects;
using Users.Shared.Authorization;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Commands.AdminCreateReservation;

internal sealed class AdminCreateReservationCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    IReservationChecker reservationChecker,
    IPricingModuleApi pricingModuleApi,
    IFacilityAuthorizationService facilityAuthorizationService,
    IFacilitiesModuleApi facilitiesModuleApi)
    : IRequestHandler<AdminCreateReservationCommand, Guid>
{
    public async Task<Guid> Handle(AdminCreateReservationCommand request, CancellationToken cancellationToken)
    {
        var facilityId = await facilitiesModuleApi.GetFacilityIdByCourtIdAsync(request.CourtId, cancellationToken);
        if (facilityId is null)
            throw new InvalidOperationException("Court not found.");

        facilityAuthorizationService.AuthorizeFacilityAccess(facilityId.Value);

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

        var price = await pricingModuleApi.CalculatePriceAsync(facilityId.Value, request.CourtId, request.StartTime, request.EndTime, cancellationToken);

        var reservation = Reservation.Create(request.UserId, request.CourtId, timeRange, price);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
