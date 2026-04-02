using FluentValidation;
using MediatR;

namespace Users.Application.Accounts.Commands.PasswordReset;

public sealed record GeneratePasswordResetTokenCommand(string Email) : IRequest;

public sealed class GeneratePasswordResetTokenCommandValidator : AbstractValidator<GeneratePasswordResetTokenCommand>
{
    public GeneratePasswordResetTokenCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
    }
}
