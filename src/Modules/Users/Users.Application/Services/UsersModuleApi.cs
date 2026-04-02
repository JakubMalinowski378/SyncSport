using Shared.Persistence;
using Users.Domain.Entities;
using Users.Shared;
using Users.Shared.DTOs;

namespace Users.Application.Services;

internal sealed class UsersModuleApi(IRepository<User, Guid> userRepository) : IUsersModuleApi
{
    public async Task<bool> ExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return await userRepository.AnyAsync(u => u.Id == userId, ct: cancellationToken);
    }

    public async Task<UserDetailsDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByIdAsync(userId, ct: cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new UserDetailsDto(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            $"{user.Name.FirstName} {user.Name.LastName}",
            user.Role.ToString(),
            user.IsActive,
            user.ManagedFacilityIds);
    }
}
