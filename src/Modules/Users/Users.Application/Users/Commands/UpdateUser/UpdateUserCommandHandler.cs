using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;
using Users.Domain.ValueObjects;

namespace Users.Application.Users.Commands.UpdateUser;

internal sealed class UpdateUserCommandHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<UpdateUserCommand>
{
    public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(request.UserId, ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        user.UpdateName(FullName.Create(request.FirstName, request.LastName));

        userRepository.Update(user);
        await userRepository.SaveChangesAsync(cancellationToken);
    }
}
