using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Queries.GetUser;

internal sealed class GetUserQueryHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<GetUserQuery, GetUserResponse?>
{
    public async Task<GetUserResponse?> Handle(GetUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
            request.UserId,
            asNoTracking: true,
            ct: cancellationToken);

        if (user is null)
        {
            return null;
        }

        return new GetUserResponse(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.Role.ToString(),
            user.IsActive,
            user.ManagedFacilityIds
        );
    }
}
