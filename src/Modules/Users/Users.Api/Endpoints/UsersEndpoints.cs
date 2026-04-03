using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

using Users.Application.Users.Queries.GetCurrentUser;

namespace Users.Api.Endpoints;

public sealed class UsersEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();
    }

    private static async Task<IResult> GetCurrentUser(ISender sender)
    {
        var response = await sender.Send(new GetCurrentUserQuery());

        return Results.Ok(response);
    }
}
