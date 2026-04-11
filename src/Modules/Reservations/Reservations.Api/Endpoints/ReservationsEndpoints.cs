using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc;
using Reservations.Application.Reservations.Commands.AdminCreateReservation;
using Reservations.Application.Reservations.Commands.AdminDeleteReservation;        
using Reservations.Application.Reservations.Commands.CancelReservation;
using Reservations.Application.Reservations.Commands.CreateReservation;
using Reservations.Application.Reservations.Queries.GetAvailableSlots;
using Reservations.Application.Reservations.Queries.GetCourtReservations;       
using Reservations.Application.Reservations.Queries.GetReservation;
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

        group.MapDelete("/{id:guid}/facility/{facilityId:guid}", AdminDeleteReservation)
            .WithName("AdminDeleteReservation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization($"{Policies.Admin}, {Policies.Manager}");

        group.MapDelete("/me/{id:guid}", CancelSelfReservation)
            .WithName("CancelSelfReservation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.User);

        group.MapGet("/courts/{courtId:guid}", GetCourtReservations)
            .WithName("GetCourtReservations")
            .Produces<IReadOnlyCollection<CourtReservationResponse>>(StatusCodes.Status200OK);

        group.MapGet("/users/{userId:guid}", GetReservationsByUserId)
            .WithName("GetReservationsByUserId")
            .Produces<PagedResult<UserReservationResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization($"{Policies.Admin}, {Policies.Manager}");

        group.MapGet("/{id:guid}", GetReservationById)
            .WithName("GetReservationById")
.Produces<ReservationDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        var availabilityGroup = app.MapGroup("/api/facilities").WithTags("Availability");

        availabilityGroup.MapGet("/{id:guid}/available-slots", GetAvailableSlots)
            .WithName("GetAvailableSlots")
            .Produces<AvailableSlotsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);
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

    private static async Task<IResult> AdminDeleteReservation(
        Guid id,
        Guid facilityId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AdminDeleteReservationCommand(id, facilityId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> CancelSelfReservation(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new CancelReservationCommand(id), cancellationToken);
        return Results.NoContent();
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

private static async Task<IResult> GetReservationById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetReservationQuery(id), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);        
    }

    private static async Task<IResult> GetAvailableSlots(
        Guid id,
        DateOnly date,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetAvailableSlotsQuery(id, date), cancellationToken);
        return Results.Ok(result);
    }
}
