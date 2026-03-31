using Shared.Domain.Exceptions;

namespace Users.Domain.Exceptions;

public sealed class InvalidEmailException : DomainException
{
    public InvalidEmailException(string value)
        : base($"The email '{value}' is invalid.")
    {
    }

    public InvalidEmailException() 
        : base("Email address cannot be empty.")
    {
    }
}
