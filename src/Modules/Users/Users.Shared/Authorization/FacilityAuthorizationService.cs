using Shared.Domain.Enums;

namespace Users.Shared.Authorization;

public sealed class FacilityAuthorizationService(ICurrentUser currentUser) : IFacilityAuthorizationService
{
    public void AuthorizeFacilityAccess(Guid facilityId)
    {
        var currentUserState = currentUser.GetState();

        if (currentUserState.Role == UserRole.Manager.ToString())
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