using MediatR;
using Shared.Pagination;
using Shared.Persistence;
using Users.Application.Users.Queries.GetUser;
using Users.Domain.Entities;

namespace Users.Application.Users.Queries.GetUsers;

internal sealed class GetUsersQueryHandler(
    IRepository<User, Guid> userRepository)
    : IRequestHandler<GetUsersQuery, PagedResult<GetUserResponse>>
{
    public async Task<PagedResult<GetUserResponse>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, totalCount) = await userRepository.GetPagedAsync(
            request.PageNumber,
            request.PageSize,
            asNoTracking: true,
            ct: cancellationToken);

        var responseItems = items.Select(user => new GetUserResponse(
            user.Id,
            user.Email.Value,
            user.Name.FirstName,
            user.Name.LastName,
            user.Role.ToString(),
            user.IsActive,
            user.ManagedFacilityIds
        )).ToList();

        return new PagedResult<GetUserResponse>(
            responseItems,
            totalCount,
            request.PageNumber,
            request.PageSize);
    }
}
