using MediatR;
using Reservations.Domain.Entities;
using Reservations.Domain.Exceptions;
using Reservations.Domain.Services;
using Reservations.Domain.ValueObjects;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Commands.CreateReservation;

internal sealed class CreateReservationCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    IReservationChecker reservationChecker)
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

        var reservation = Reservation.Create(request.UserId, request.CourtId, timeRange);

        await reservationRepository.AddAsync(reservation, cancellationToken);
        await reservationRepository.SaveChangesAsync(cancellationToken);

        return reservation.Id;
    }
}
