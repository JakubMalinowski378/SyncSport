using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Reservations.Application.Reservations.Commands.AdminCreateReservation;
using Reservations.Application.Reservations.Commands.AdminDeleteReservation;
using Reservations.Application.Reservations.Commands.CancelReservation;
using Reservations.Application.Reservations.Commands.CreateReservation;
using Reservations.Application.Reservations.Queries.GetCourtReservations;
using Reservations.Application.Reservations.Queries.GetMyReservations;
using Reservations.Application.Reservations.Queries.GetReservation;
using Reservations.Application.Reservations.Queries.GetReservationsByUserId;
using Reservations.Application.Reservations.Queries.GetUserReservations;
using Reservations.Domain.Enums;
using Shared.Authorization;
using Shared.Pagination;

namespace Reservations.Api.Endpoints;

public sealed class ReservationsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/reservations").WithTags("Reservations");
        var userGroup = app.MapGroup("/api/users").WithTags("Reservations");

        group.MapGet("/me", GetMyReservations)
            .WithName("GetMyReservations")
            .Produces<PagedResult<ReservationWithDetailsDto>>(StatusCodes.Status200OK)
            .RequireAuthorization();

        group.MapPost("/me", CreateReservation)
            .WithName("CreateSelfReservation")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapPost("/", AdminCreateReservation)
            .WithName("AdminCreateReservation")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapDelete("/{id:guid}/facility/{facilityId:guid}", AdminDeleteReservation)
            .WithName("AdminDeleteReservation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapDelete("/me/{id:guid}", CancelSelfReservation)
            .WithName("CancelSelfReservation")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();

        group.MapGet("/courts/{courtId:guid}", GetCourtReservations)
            .WithName("GetCourtReservations")
            .Produces<GetCourtReservationsResponse>(StatusCodes.Status200OK);

        group.MapGet("/users/{userId:guid}", GetReservationsByUserId)
            .WithName("GetReservationsByUserId")
            .Produces<PagedResult<UserReservationResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapGet("/{id:guid}", GetReservationById)
            .WithName("GetReservationById")
            .Produces<ReservationDetailsResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        userGroup.MapGet("{userId:guid}/reservations", GetUserReservations)
            .WithName("GetUserReservations")
            .Produces<PagedResult<UserReservationResponse>>(StatusCodes.Status200OK)
            .RequireAuthorization(Policies.AdminOrManager);
    }
    private static async Task<IResult> GetMyReservations(
        [AsParameters] GetMyReservationsRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetMyReservationsQuery
        {
            PageNumber = request.PageNumber,
            PageSize = request.PageSize,
            SortBy = request.SortBy,
            SortDirection = request.SortDirection,
            Status = request.Status,
            CourtName = request.CourtName,
            FacilityName = request.FacilityName
        };
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
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
        [FromQuery] Guid userId,
        [AsParameters] GetUserReservationsQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        query.UserId = userId;
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetCourtReservations(
        Guid courtId,
        [FromQuery] DateOnly weekDate,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetCourtReservationsQuery(courtId, weekDate);
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
}

internal sealed record GetMyReservationsRequest(
    int PageNumber = 1,
    int PageSize = 10,
    string? SortBy = null,
    string? SortDirection = null,
    ReservationStatus? Status = null,
    string? CourtName = null,
    string? FacilityName = null
);
