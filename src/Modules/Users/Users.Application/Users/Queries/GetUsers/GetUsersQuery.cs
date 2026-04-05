using MediatR;
using Shared.Pagination;
using Users.Application.Users.Queries.GetUser;

namespace Users.Application.Users.Queries.GetUsers;

public sealed record GetUsersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<PagedResult<GetUserResponse>>;
