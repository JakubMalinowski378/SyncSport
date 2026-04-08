using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Reservations.Commands.CancelReservation;

internal sealed class CancelReservationCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    ICurrentUser currentUser)
    : IRequestHandler<CancelReservationCommand>
{
    public async Task Handle(CancelReservationCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.Id, ct: cancellationToken);
        
        if (reservation is null)
        {
            throw new Exception("Reservation not found.");
        }

        var userId = currentUser.GetState().UserId;

        if (reservation.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only cancel your own reservations.");
        }

        var timeUntilReservation = reservation.Time.Start - DateTime.UtcNow;

        if (timeUntilReservation.TotalHours < 24)
        {
            throw new Exception("Reservation can only be cancelled at least 24 hours before it starts.");
        }

        reservation.Cancel();
        
        // reservationRepository.Update(reservation); optionally if tracking is explicit, though typically SaveChanges is enough if entity is tracked.
        await reservationRepository.SaveChangesAsync(cancellationToken);
    }
}
