namespace Users.Application.Abstractions;

public record struct RefreshTokenResult(string Token, DateTimeOffset ExpiryTime);
