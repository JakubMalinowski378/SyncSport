using FluentValidation;
using MediatR;
using Users.Application.Accounts.Common;

namespace Users.Application.Accounts.Commands.SignIn;

public sealed record SignInCommand(
    string Email,
    string Password)
    : IRequest<AuthenticationResponse>;

public sealed class SignInCommandValidator : AbstractValidator<SignInCommand>
{
    public SignInCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
