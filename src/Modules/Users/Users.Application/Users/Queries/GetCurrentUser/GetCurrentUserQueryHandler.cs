using MediatR;
using Shared.Persistence;
using Users.Domain.Entities;

namespace Users.Application.Users.Queries.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<GetCurrentUserQuery, GetCurrentUserResponse>
{
    public async Task<GetCurrentUserResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByIdAsync(
            request.UserId,
            asNoTracking: true,
            ct: cancellationToken);

        if (user is null)
        {
            throw new Exception($"User with id {request.UserId} not found");
        }

        return new GetCurrentUserResponse(
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