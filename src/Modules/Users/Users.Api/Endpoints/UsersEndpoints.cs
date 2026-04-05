using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorization;
using Shared.Pagination;
using Users.Application.Users.Commands.AssignFacilityToUser;
using Users.Application.Users.Commands.ChangeUserRole;
using Users.Application.Users.Commands.DeleteUser;
using Users.Application.Users.Commands.RemoveFacilityAssignmentFromUser;
using Users.Application.Users.Commands.UpdateCurrentUser;
using Users.Application.Users.Commands.UpdateUser;
using Users.Application.Users.Queries.GetCurrentUser;
using Users.Application.Users.Queries.GetUser;

namespace Users.Api.Endpoints;

public sealed class UsersEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet(string.Empty, GetUsers)
            .WithName("GetUsers")
            .Produces<PagedResult<GetUserResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.Admin);

        group.MapGet("me", GetCurrentUser)
            .WithName("GetCurrentUser")
            .Produces<GetCurrentUserResponse>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapGet("me/reservations", GetCurrentUserReservations)
            .WithName("GetCurrentUserReservations")
            .Produces(StatusCodes.Status200OK);

        group.MapGet("{id:guid}", GetUser)
            .WithName("GetUser")
            .Produces<GetUserResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("{id:guid}/facility-assignments", AssignFacilityToUser)
            .WithName("AssignFacilityToUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("me", UpdateCurrentUser)
            .WithName("UpdateCurrentUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapPut("{id:guid}", UpdateUser)
            .WithName("UpdateUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Policies.Admin);

        group.MapPatch("{id:guid}/role", ChangeUserRole)
            .WithName("ChangeUserRole")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Policies.Admin);

        group.MapDelete("{id:guid}", DeleteUser)
            .WithName("DeleteUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Policies.Admin);

        group.MapDelete("{id:guid}/facility-assignments/{facilityId:guid}", RemoveFacilityAssignmentFromUser)
            .WithName("RemoveFacilityAssignmentFromUser")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> GetUsers(ISender sender, int pageNumber = 1, int pageSize = 10)
    {
        var result = await sender.Send(new Users.Application.Users.Queries.GetUsers.GetUsersQuery(pageNumber, pageSize));
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCurrentUser(ISender sender)
    {
        var response = await sender.Send(new GetCurrentUserQuery());

        return Results.Ok(response);
    }

    private static async Task<IResult> GetCurrentUserReservations(ISender sender)
    {
        // TODO
        await Task.CompletedTask;
        return Results.Ok();
    }

    private static async Task<IResult> GetUser(Guid id, ISender sender)
    {
        var response = await sender.Send(new GetUserQuery(id));

        if (response is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(response);
    }

    private static async Task<IResult> AssignFacilityToUser(Guid id, AssignFacilityToUserRequest request, ISender sender)
    {
        await sender.Send(new AssignFacilityToUserCommand(id, request.FacilityId));
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateCurrentUser(UpdateCurrentUserCommand command, ISender sender)
    {
        await sender.Send(command);
        return Results.NoContent();
    }

    private static async Task<IResult> UpdateUser(Guid id, UpdateUserRequest request, ISender sender)
    {
        await sender.Send(new UpdateUserCommand(id, request.FirstName, request.LastName));
        return Results.NoContent();
    }

    private static async Task<IResult> ChangeUserRole(Guid id, ChangeUserRoleRequest request, ISender sender)
    {
        await sender.Send(new ChangeUserRoleCommand(id, request.Role));
        return Results.NoContent();
    }

    private static async Task<IResult> DeleteUser(Guid id, ISender sender)
    {
        await sender.Send(new DeleteUserCommand(id));
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveFacilityAssignmentFromUser(Guid id, Guid facilityId, ISender sender)
    {
        await sender.Send(new RemoveFacilityAssignmentFromUserCommand(id, facilityId));
        return Results.NoContent();
    }
}
