namespace Users.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public required string Issuer { get; init; }
    public required string Audience { get; init; }
    public required string Secret { get; init; }
    public required int ExpiryMinutes { get; init; }
}
