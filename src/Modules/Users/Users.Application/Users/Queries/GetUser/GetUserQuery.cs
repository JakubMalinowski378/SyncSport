using MediatR;

namespace Users.Application.Users.Queries.GetUser;

public sealed record GetUserQuery(Guid UserId) : IRequest<GetUserResponse?>;
