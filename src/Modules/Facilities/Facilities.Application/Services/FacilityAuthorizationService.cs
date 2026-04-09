using Facilities.Application.Abstractions;
using Shared.Domain.Enums;
using Users.Shared;

namespace Facilities.Application.Services;

internal sealed class FacilityAuthorizationService(ICurrentUser currentUser) : IFacilityAuthorizationService
{
    public void AuthorizeFacilityAccess(Guid facilityId)
    {
        var currentUserState = currentUser.GetState();

        if (currentUserState.Role == UserRole.Manager.ToString() || currentUserState.Role == "Moderator")
        {
            var isManagerOfFacility = currentUserState.ManagedFacilityIds.Contains(facilityId);

            if (!isManagerOfFacility)
            {
                throw new UnauthorizedAccessException("You are not authorized to access this facility.");
            }
        }
        else if (currentUserState.Role != UserRole.Admin.ToString())
        {
            throw new UnauthorizedAccessException("You do not have permission to perform this action.");
        }
    }
}
