using FluentValidation;
using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Commands.RemoveFacilityAssignmentFromUser;

public sealed record RemoveFacilityAssignmentFromUserCommand(Guid UserId, Guid FacilityId) : IRequest;

public sealed class RemoveFacilityAssignmentFromUserCommandValidator : AbstractValidator<RemoveFacilityAssignmentFromUserCommand>
{
    public RemoveFacilityAssignmentFromUserCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FacilityId).NotEmpty();
    }
}

internal sealed class RemoveFacilityAssignmentFromUserCommandHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<RemoveFacilityAssignmentFromUserCommand>
{
    public async Task Handle(RemoveFacilityAssignmentFromUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        user.RemoveFacilityAssignment(request.FacilityId);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
