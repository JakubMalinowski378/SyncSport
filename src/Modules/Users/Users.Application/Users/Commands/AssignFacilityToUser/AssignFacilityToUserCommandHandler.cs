using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Commands.AssignFacilityToUser;

internal sealed class AssignFacilityToUserCommandHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<AssignFacilityToUserCommand>
{
    public async Task Handle(AssignFacilityToUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        user.AssignToFacility(request.FacilityId);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
