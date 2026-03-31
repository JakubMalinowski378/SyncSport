using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Users.Application.Abstractions;
using Users.Domain.Entities;

namespace Users.Infrastructure.Authentication;

internal sealed class JwtService(IOptions<JwtOptions> options) : IJwtService
{
    private readonly JwtOptions _options = options.Value;

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email.Value),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (user.ManagedFacilityIds.Any())
        {
            claims.AddRange(user.ManagedFacilityIds
                .Select(id => new Claim("ManagedFacilityId", id.ToString())));
        }

        var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
        var credentials = new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_options.ExpiryMinutes),
            Issuer = _options.Issuer,
            Audience = _options.Audience,
            SigningCredentials = credentials
        };

        var handler = new JwtSecurityTokenHandler();
        var token = handler.CreateToken(tokenDescriptor);

        return handler.WriteToken(token);
    }

    public RefreshTokenResult GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);

        return new RefreshTokenResult(token, DateTime.UtcNow.AddDays(_options.RefreshTokenExpiryDays));
    }

    public int GetPasswordResetTokenExpiryMinutes() => _options.PasswordResetTokenExpiryMinutes;
}
