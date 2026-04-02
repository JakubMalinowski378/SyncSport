using FluentValidation;
using MediatR;
using Users.Application.Validation;

namespace Users.Application.Accounts.Commands.PasswordReset;

public sealed record ResetPasswordCommand(string ResetToken, string NewPassword) : IRequest;

public sealed class ResetPasswordCommandValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordCommandValidator()
    {
        RuleFor(x => x.ResetToken).NotEmpty();
        RuleFor(x => x.NewPassword).Password();
    }
}
