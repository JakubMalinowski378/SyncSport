namespace Users.Application.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    IReadOnlyCollection<Guid> ManagedFacilityIds
);