using FluentValidation;
using MediatR;
using Shared.Domain.Enums;

namespace Users.Application.Users.Commands.ChangeUserRole;

public sealed record ChangeUserRoleCommand(Guid UserId, UserRole Role) : IRequest;

public sealed class ChangeUserRoleCommandValidator : AbstractValidator<ChangeUserRoleCommand>
{
    public ChangeUserRoleCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Role).IsInEnum();
    }
}