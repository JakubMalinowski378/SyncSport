using Users.Shared.DTOs;

namespace Users.Shared;

public interface IUsersModuleApi
{
    Task<UserDetailsDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}
