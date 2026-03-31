namespace Users.Application.Accounts.Common;

public sealed record AuthenticationResponse(string JwtToken, string RefreshToken);
