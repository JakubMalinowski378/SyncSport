using FluentValidation;
using MediatR;
using Users.Application.Accounts.Common;
using Users.Application.Validation;

namespace Users.Application.Accounts.Commands.SignUp;

public sealed record SignUpCommand(
    string Email,
    string Password,
    string FirstName,
    string LastName)
    : IRequest<AuthenticationResponse>;

public sealed class SignUpCommandValidator : AbstractValidator<SignUpCommand>
{
    public SignUpCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).Password();
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(50);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(50);
    }
}
