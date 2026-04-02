using Shared.Domain.Exceptions;

namespace Users.Domain.Exceptions;

public sealed class InvalidPasswordResetTokenException()
    : DomainException("Invalid or expired password reset token.");
