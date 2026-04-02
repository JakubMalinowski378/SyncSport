using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Users.Application.Accounts.Commands.Logout;
using Users.Application.Accounts.Commands.PasswordReset;
using Users.Application.Accounts.Commands.Refresh;
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

        group.MapPost("refresh-token", RefreshToken)
            .WithName("RefreshToken")
            .Produces<AuthenticationResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("logout", Logout)
            .WithName("Logout")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("forgot-password", GeneratePasswordResetToken)
            .WithName("GeneratePasswordResetToken")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("reset-password", ResetPassword)
            .WithName("ResetPassword")
            .Produces(StatusCodes.Status204NoContent)
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

    private static async Task<IResult> RefreshToken(RefreshTokenCommand command, ISender sender)
    {
        var response = await sender.Send(command);

        return Results.Ok(response);
    }

    private static async Task<IResult> Logout(LogoutCommand command, ISender sender)
    {
        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> GeneratePasswordResetToken(GeneratePasswordResetTokenCommand command, ISender sender)
    {
        await sender.Send(command);

        return Results.NoContent();
    }

    private static async Task<IResult> ResetPassword(ResetPasswordCommand command, ISender sender)
    {
        await sender.Send(command);

        return Results.NoContent();
    }
}
