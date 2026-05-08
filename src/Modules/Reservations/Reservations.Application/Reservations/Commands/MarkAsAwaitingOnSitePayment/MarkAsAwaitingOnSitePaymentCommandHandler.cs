using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Reservations.Commands.MarkAsAwaitingOnSitePayment;

internal sealed class MarkAsAwaitingOnSitePaymentCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    ICurrentUser currentUser)
    : IRequestHandler<MarkAsAwaitingOnSitePaymentCommand>
{
    public async Task Handle(MarkAsAwaitingOnSitePaymentCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.Id, ct: cancellationToken);

        if (reservation is null)
        {
            throw new Exception("Reservation not found.");
        }

        var userId = currentUser.GetState().UserId;

        if (reservation.UserId != userId)
        {
            throw new UnauthorizedAccessException("You can only modify your own reservations.");
        }

        reservation.MarkAsAwaitingOnSitePayment();

        await reservationRepository.SaveChangesAsync(cancellationToken);
    }
}