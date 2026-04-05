using MediatR;
using Shared.Domain.Exceptions;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Commands.ChangeUserRole;

internal sealed class ChangeUserRoleCommandHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<ChangeUserRoleCommand>
{
    public async Task Handle(ChangeUserRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        if (user.Role == request.Role)
        {
            return;
        }

        user.ChangeRole(request.Role);

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
