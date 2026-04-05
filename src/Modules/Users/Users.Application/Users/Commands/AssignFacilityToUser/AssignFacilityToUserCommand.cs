using FluentValidation;
using MediatR;

namespace Users.Application.Users.Commands.AssignFacilityToUser;

public sealed record AssignFacilityToUserCommand(Guid UserId, Guid FacilityId) : IRequest;

public sealed class AssignFacilityToUserCommandValidator : AbstractValidator<AssignFacilityToUserCommand>
{
    public AssignFacilityToUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}
