using Shared.Domain.Exceptions;

namespace Users.Domain.Exceptions;

public sealed class EmailAlreadyTakenException(string email)
    : DomainException($"The email '{email}' is already taken.");
