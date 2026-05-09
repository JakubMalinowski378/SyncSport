using Facilities.Shared;
using MediatR;
using Pricing.Shared;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;
using Shared.Time;
using Users.Shared.Authorization;

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

        var startUtc = NormalizeToUtc(request.StartTime);
        if (startUtc < DateTimeOffset.UtcNow)
        {
            throw new ReservationInPastException();
        }

        await EnsureAlignedWithFacilityAsync(request, cancellationToken);

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

        if (request.PayOnSite)
        {
            reservation.MarkAsAwaitingOnSitePayment();
        }

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }

    private static DateTimeOffset NormalizeToUtc(DateTimeOffset dateTime)
    {
        return dateTime.ToUniversalTime();
    }

    private async Task EnsureAlignedWithFacilityAsync(AdminCreateReservationCommand command, CancellationToken ct)
    {
        var facilityId = await facilitiesModuleApi.GetFacilityIdByCourtIdAsync(command.CourtId, ct);
        if (facilityId is null)
        {
            throw new InvalidOperationException("Facility not found for the given court.");
        }

        var info = await facilitiesModuleApi.GetFacilityAvailabilityInfoAsync(facilityId.Value, ct);
        if (info is null)
        {
            throw new InvalidOperationException("Facility availability info not found.");
        }

        var court = info.Courts.FirstOrDefault(c => c.CourtId == command.CourtId);
        if (court is null)
        {
            throw new InvalidOperationException("Court not found in facility.");
        }

        var duration = court.ReservationDurationMinutes;

        var start = command.StartTime;
        var end = command.EndTime;

        if ((end - start).TotalMinutes != duration)
        {
            throw new InvalidOperationException("Reservation duration does not match the court's reservation duration.");
        }

        var startLocal = PolishTimeProvider.ConvertUtcToPolishLocal(start.ToUniversalTime());

        var opening = info.OpeningHours.FirstOrDefault(o => o.DayOfWeek == startLocal.DayOfWeek);
        if (opening is null)
        {
            throw new InvalidOperationException("Facility is closed on this day.");
        }

        var open = opening.OpenTime;
        var close = opening.CloseTime;

        if (!open.HasValue || !close.HasValue)
        {
            throw new InvalidOperationException("Facility is closed on this day.");
        }

        var startTime = TimeOnly.FromTimeSpan(startLocal.TimeOfDay);
        var endTime = TimeOnly.FromTimeSpan(PolishTimeProvider.ConvertUtcToPolishLocal(end.ToUniversalTime()).TimeOfDay);

        if (startTime < open.Value || endTime > close.Value)
        {
            throw new InvalidOperationException("Reservation must be within facility opening hours.");
        }

        var minutesFromOpen = (startTime.ToTimeSpan() - open.Value.ToTimeSpan()).TotalMinutes;
        if (minutesFromOpen % duration != 0)
        {
            throw new InvalidOperationException("Reservation start time must align with the court's reservation slots.");
        }
    }
}
