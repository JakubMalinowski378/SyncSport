using Users.Domain.Entities;

namespace Users.Application.Abstractions;

public interface IJwtService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
}
