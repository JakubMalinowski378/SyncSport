using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Commands.ChangeUserStatus;

internal sealed class ChangeUserStatusCommandHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<ChangeUserStatusCommand>
{
    public async Task Handle(ChangeUserStatusCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        if (request.IsActive)
        {
            user.Activate();
        }
        else
        {
            user.Deactivate();
        }

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}