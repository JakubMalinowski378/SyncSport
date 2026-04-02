namespace Users.Shared.DTOs;

public sealed record UserDetailsDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string FullName,
    string Role,
    bool IsActive,
    IReadOnlyCollection<Guid> ManagedFacilityIds);
