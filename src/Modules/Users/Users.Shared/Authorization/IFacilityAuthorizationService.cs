namespace Users.Shared.Authorization;

public interface IFacilityAuthorizationService
{
    void AuthorizeFacilityAccess(Guid facilityId);
}