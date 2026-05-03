using MediatR;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Commands.MarkReservationAsPaidOnSite;

internal sealed class MarkReservationAsPaidOnSiteCommandHandler(
    IRepository<Reservation, Guid> reservationRepository)
    : IRequestHandler<MarkReservationAsPaidOnSiteCommand>
{
    public async Task Handle(MarkReservationAsPaidOnSiteCommand request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.Id, ct: cancellationToken);

        if (reservation is null)
        {
            throw new KeyNotFoundException("Reservation not found.");
        }

        reservation.MarkAsPaidOnSite();

        await reservationRepository.SaveChangesAsync(cancellationToken);
    }
}
