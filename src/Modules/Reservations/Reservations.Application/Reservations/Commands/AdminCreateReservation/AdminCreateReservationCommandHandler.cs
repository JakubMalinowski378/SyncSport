using MediatR;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Reservations.Domain.ValueObjects;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Reservations.Commands.AdminCreateReservation;

internal sealed class AdminCreateReservationCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    IReservationChecker reservationChecker,
    ICurrentUser currentUser)
    : IRequestHandler<AdminCreateReservationCommand, Guid>
{
    public async Task<Guid> Handle(AdminCreateReservationCommand request, CancellationToken cancellationToken)
    {
        var currentUserState = currentUser.GetState();

        if (currentUserState.Role == UserRole.Manager.ToString())
        {
            var isManagerOfFacility = currentUserState.ManagedFacilityIds.Contains(request.FacilityId);

            if (!isManagerOfFacility)
            {
                throw new UnauthorizedAccessException("You are not authorized to create a reservation for this facility.");
            }
        }

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

        var reservation = Reservation.Create(request.UserId, request.CourtId, timeRange);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
