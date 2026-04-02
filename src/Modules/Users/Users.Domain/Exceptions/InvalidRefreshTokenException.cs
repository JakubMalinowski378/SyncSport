using Shared.Domain.Exceptions;

namespace Users.Domain.Exceptions;

public sealed class InvalidRefreshTokenException()
    : DomainException("Invalid refresh token.");
