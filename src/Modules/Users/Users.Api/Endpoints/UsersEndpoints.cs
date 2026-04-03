using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Users.Application.Users.Commands.UpdateCurrentUser;
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

        group.MapPut("me", UpdateCurrentUser)
            .WithName("UpdateCurrentUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapGet("me/reservations", GetCurrentUserReservations)
            .WithName("GetCurrentUserReservations")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> GetCurrentUser(ISender sender)
    {
        var response = await sender.Send(new GetCurrentUserQuery());

        return Results.Ok(response);
    }

    private static async Task<IResult> UpdateCurrentUser(UpdateCurrentUserCommand command, ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> GetCurrentUserReservations(ISender sender)
    {
        // TODO
        await Task.CompletedTask;
        return Results.Ok();
    }
}
