using Microsoft.Extensions.Options;
using Users.Application.Abstractions;

namespace Users.Infrastructure.Authentication;

internal sealed class PasswordHasher(
    IOptions<PasswordHasherOptions> options)
    : IPasswordHasher
{
    private readonly PasswordHasherOptions _options = options.Value;
    public string Hash(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, _options.WorkFactor);
    }

    public bool Verify(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
