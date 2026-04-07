namespace Users.Shared;

public record CurrentUserState(
    Guid UserId,
    string Email,
    string Role,
    IReadOnlyCollection<Guid> ManagedFacilityIds,
    bool IsAuthenticated);

public interface ICurrentUser
{
    CurrentUserState GetState();
}
