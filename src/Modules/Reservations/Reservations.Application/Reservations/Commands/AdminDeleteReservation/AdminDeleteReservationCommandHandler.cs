using MediatR;
using Reservations.Application.Abstractions;
using Reservations.Domain.Entities;
using Shared.Persistence;

namespace Reservations.Application.Reservations.Commands.AdminDeleteReservation;

internal sealed class AdminDeleteReservationCommandHandler(
    IRepository<Reservation, Guid> reservationRepository,
    IFacilityAuthorizationService facilityAuthorizationService)
    : IRequestHandler<AdminDeleteReservationCommand>
{
    public async Task Handle(AdminDeleteReservationCommand request, CancellationToken cancellationToken)
    {
        facilityAuthorizationService.AuthorizeFacilityAccess(request.FacilityId);

        var reservation = await reservationRepository.GetByIdAsync(request.Id, ct: cancellationToken);
        if (reservation is null)
        {
            throw new Exception("Reservation not found.");
        }

        reservationRepository.Remove(reservation);
        await reservationRepository.SaveChangesAsync(cancellationToken);
    }
}
