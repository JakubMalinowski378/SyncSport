using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Reservations.Application.Reservations.Commands.AdminCreateReservation;
using Reservations.Application.Reservations.Commands.CreateReservation;
using Reservations.Application.Reservations.Queries.GetCourtReservations;
using Reservations.Application.Reservations.Queries.GetReservationsByUserId;
using Reservations.Application.Reservations.Queries.GetUserReservations;
using Shared.Authorization;
using Shared.Pagination;

namespace Reservations.Api.Endpoints;

public sealed class ReservationsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations").WithTags("Reservations");

        group.MapPost("/me", CreateReservation)
            .WithName("CreateSelfReservation")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.User);

        group.MapPost("/", AdminCreateReservation)
            .WithName("AdminCreateReservation")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization($"{Policies.Admin}, {Policies.Manager}");

        group.MapGet("/me", GetUserReservations)
            .WithName("GetUserReservations")
            .Produces<PagedResult<ReservationResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization(Policies.User);

        group.MapGet("/courts/{courtId:guid}", GetCourtReservations)
            .WithName("GetCourtReservations")
            .Produces<IReadOnlyCollection<CourtReservationResponse>>(StatusCodes.Status200OK);

        group.MapGet("/users/{userId:guid}", GetReservationsByUserId)
            .WithName("GetReservationsByUserId")
            .Produces<PagedResult<UserReservationResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization($"{Policies.Admin}, {Policies.Manager}");
    }

    private static async Task<IResult> CreateReservation(
        CreateReservationCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/reservations/{id}", id);
    }

    private static async Task<IResult> AdminCreateReservation(
        AdminCreateReservationCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var id = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/reservations/{id}", id);
    }

    private static async Task<IResult> GetUserReservations(
        [AsParameters] GetUserReservationsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCourtReservations(
        [AsParameters] GetCourtReservationsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetReservationsByUserId(
        [AsParameters] GetReservationsByUserIdQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}
