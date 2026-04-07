using Facilities.Shared;
using MediatR;
using Reservations.Domain.Entities;
using Shared.Domain.Enums;
using Shared.Persistence;
using Users.Shared;

namespace Reservations.Application.Reservations.Queries.GetReservation;

internal sealed class GetReservationQueryHandler(
    IRepository<Reservation, Guid> reservationRepository,
    ICurrentUser currentUser,
    IFacilitiesModuleApi facilitiesModuleApi)
    : IRequestHandler<GetReservationQuery, ReservationDetailsResponse?>
{
    public async Task<ReservationDetailsResponse?> Handle(GetReservationQuery request, CancellationToken cancellationToken)
    {
        var reservation = await reservationRepository.GetByIdAsync(request.Id, asNoTracking: true, ct: cancellationToken);

        if (reservation is null)
        {
            return null;
        }

        var userState = currentUser.GetState();

        if (userState.Role == UserRole.User.ToString())
        {
            if (reservation.UserId != userState.UserId)
            {
                return null;
            }
        }
        else if (userState.Role == UserRole.Manager.ToString())
        {
            var facilityIdOrNull = await facilitiesModuleApi.GetFacilityIdByCourtIdAsync(reservation.CourtId, cancellationToken);
            if (!facilityIdOrNull.HasValue)
            {
                return null;
            }

            if (!userState.ManagedFacilityIds.Contains(facilityIdOrNull.Value))
            {
                return null;
            }
        }

        return new ReservationDetailsResponse(
            reservation.Id,
            reservation.UserId,
            reservation.CourtId,
            reservation.Time.Start,
            reservation.Time.End);
    }
}