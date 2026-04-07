using Reservations.Application.Abstractions;
using Shared.Domain.Enums;
using Users.Shared;

namespace Reservations.Application.Services;

internal sealed class FacilityAuthorizationService(ICurrentUser currentUser) : IFacilityAuthorizationService
{
    public void AuthorizeFacilityAccess(Guid facilityId)
    {
        var currentUserState = currentUser.GetState();

        if (currentUserState.Role == UserRole.Manager.ToString())
        {
            var isManagerOfFacility = currentUserState.ManagedFacilityIds.Contains(facilityId);

            if (!isManagerOfFacility)
            {
                throw new UnauthorizedAccessException("You are not authorized to access for this facility.");
            }
        }
    }
}
