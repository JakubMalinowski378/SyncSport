using MediatR;
using Shared.Behaviors;
using System.Text.Json.Serialization;

namespace Users.Application.Users.Queries.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<GetCurrentUserResponse>, IAuditable
{
    [JsonIgnore]
    public Guid UserId { get; set; }
}