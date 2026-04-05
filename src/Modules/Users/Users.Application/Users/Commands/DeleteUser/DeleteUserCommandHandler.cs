using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Commands.DeleteUser;

internal sealed class DeleteUserCommandHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<DeleteUserCommand>
{
    public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        userRepository.Remove(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
