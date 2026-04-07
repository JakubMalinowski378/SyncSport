namespace Reservations.Application.Abstractions;

public interface IFacilityAuthorizationService
{
    void AuthorizeFacilityAccess(Guid facilityId);
}
