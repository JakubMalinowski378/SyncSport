namespace Users.Application.Users.Queries.GetUser;

public sealed record GetUserResponse(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string Role,
    bool IsActive,
    IReadOnlyCollection<Guid> ManagedFacilityIds
);
