using FluentValidation;
using MediatR;
using Users.Application.Accounts.Common;

namespace Users.Application.Accounts.Commands.Refresh;

public sealed record RefreshTokenCommand(
    string RefreshToken)
    : IRequest<AuthenticationResponse>;

public sealed class RefreshTokenCommandValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
