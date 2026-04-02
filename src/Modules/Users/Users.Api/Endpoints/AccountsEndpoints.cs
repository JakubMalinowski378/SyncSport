using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Users.Application.Accounts.Commands.SignIn;
using Users.Application.Accounts.Commands.SignUp;
using Users.Application.Accounts.Common;

namespace Users.Api.Endpoints;

public sealed class AccountsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/accounts").WithTags("Accounts");

        group.MapPost("sign-up", SignUp)
            .WithName("SignUp")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("sign-in", SignIn)
            .WithName("SignIn")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);
    }

    private static async Task<IResult> SignUp(SignUpCommand command, ISender sender)
    {
        var response = await sender.Send(command);

        return Results.Ok(response);
    }

    private static async Task<IResult> SignIn(SignInCommand command, ISender sender)
    {
        var response = await sender.Send(command);

        return Results.Ok(response);
    }
}
