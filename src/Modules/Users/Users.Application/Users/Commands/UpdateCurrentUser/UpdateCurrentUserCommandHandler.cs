using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;
using Users.Shared;

namespace Users.Application.Users.Commands.UpdateCurrentUser;

internal sealed class UpdateCurrentUserCommandHandler(
    IRepository<User, Guid> userRepository,
    ICurrentUser currentUser)
    : IRequestHandler<UpdateCurrentUserCommand>
{
    public async Task Handle(UpdateCurrentUserCommand request, CancellationToken cancellationToken)
    {
        var userState = currentUser.GetState();

        if(userState is null || !userState.IsAuthenticated)
        {
            throw new Exception("User is not authenticated");
        }

        var user = await userRepository.GetByIdAsync(userState.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {userState.UserId} not found");
        }

        user.UpdateName(FullName.Create(request.FirstName, request.LastName));

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
