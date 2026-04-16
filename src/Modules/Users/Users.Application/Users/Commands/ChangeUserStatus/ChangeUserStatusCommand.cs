using FluentValidation;
using MediatR;

namespace Users.Application.Users.Commands.ChangeUserStatus;

public sealed record ChangeUserStatusCommand(Guid UserId, bool IsActive) : IRequest;

public sealed class ChangeUserStatusCommandValidator : AbstractValidator<ChangeUserStatusCommand>
{
    public ChangeUserStatusCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}