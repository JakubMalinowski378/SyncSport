using Carter;
using Facilities.Application.Facilities.Commands.CreateCourt;
using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Application.Facilities.Commands.EditCourt;
using Facilities.Application.Facilities.Commands.EditFacility;
using Facilities.Application.Facilities.Queries.GetAllFacilities;
using Facilities.Application.Facilities.Queries.GetFacilityById;
using Facilities.Application.Facilities.Commands.RemoveCourt;
using Facilities.Application.Facilities.Commands.RemoveFacility;
using Facilities.Application.Facilities.Queries.GetFacilityCourtById;
using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Shared.Authorization;
using Shared.Pagination;

namespace Facilities.Api.Endpoints;

public sealed class FacilitiesEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/facilities").WithTags("Facilities");

        group.MapPost("/", CreateFacility)
            .WithName("CreateFacility")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .DisableAntiforgery()
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapGet("/", GetFacilities)
            .WithName("GetFacilities")
            .Produces<PagedResult<GetAllFacilitiesResult>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{facilitySlug}", GetFacilityById)
            .WithName("GetFacilityById")
            .Produces<GetFacilityByIdResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{facilityId:guid}", RemoveFacility)
            .WithName("RemoveFacility")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapPut("/{facilityId:guid}", EditFacility)
            .WithName("EditFacility")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager)
            .DisableAntiforgery();

        group.MapPost("/{facilityId:guid}/courts", CreateCourt)
            .WithName("CreateCourt")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager)
            .DisableAntiforgery();

        group.MapPut("/{facilityId:guid}/courts/{courtId:guid}", EditCourt)
            .WithName("EditCourt")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager)
            .DisableAntiforgery();

        group.MapGet("/{facilitySlug}/courts/{courtSlug}", GetCourtBySlug)
            .WithName("GetCourtBySlug")
            .Produces<CourtDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{facilitySlug}/courts", GetFacilityCourts)
            .WithName("GetFacilityCourts")
            .Produces<PagedResult<CourtDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{facilityId:guid}/courts/{courtId:guid}", RemoveCourt)
            .WithName("RemoveCourt")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager);
    }

    private static async Task<IResult> CreateFacility([FromForm] CreateFacilityCommand request, ISender sender, CancellationToken ct)
    {
        var facilityId = await sender.Send(request, ct);

        return Results.Created($"/api/facilities/{facilityId}", facilityId);
    }

    private static async Task<IResult> GetFacilities([AsParameters] GetAllFacilitiesCommand query, ISender sender, CancellationToken ct)
    {
        var facilities = await sender.Send(query, ct);

        return Results.Ok(facilities);
    }

    private static async Task<IResult> GetFacilityById(string facilitySlug, ISender sender, CancellationToken ct)
    {
        var facility = await sender.Send(new GetFacilityByIdQuery(facilitySlug), ct);

        if (facility is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(facility);
    }

    private static async Task<IResult> RemoveFacility(Guid facilityId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new RemoveFacilityCommand(facilityId), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> EditFacility([FromRoute] Guid facilityId, [FromForm] EditFacilityCommand request, ISender sender, CancellationToken ct)
    {
        await sender.Send(new EditFacilityCommand
        {
            FacilityId = facilityId,
            Name = request.Name,
            Address = request.Address,
            ReservationDuration = request.ReservationDuration,
            WeeklyHours = request.WeeklyHours,
            CustomDateHours = request.CustomDateHours,
            Images = request.Images,
            RemovedImageUrls = request.RemovedImageUrls,
            MainImageIndex = request.MainImageIndex
        }, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> CreateCourt([FromRoute] Guid facilityId, [FromForm] CreateCourtCommand request, ISender sender, CancellationToken ct)
    {
        var courtId = await sender.Send(new CreateCourtCommand
        {
            FacilityId = facilityId,
            Name = request.Name,
            SurfaceType = request.SurfaceType,
            OverrideReservationDuration = request.OverrideReservationDuration,
            Images = request.Images,
            MainImageIndex = request.MainImageIndex
        }, ct);

        return Results.Created($"/api/facilities/{facilityId}/courts/{courtId}", courtId);
    }

    private static async Task<IResult> EditCourt([FromRoute] Guid facilityId, [FromRoute] Guid courtId, [FromForm] EditCourtCommand request, ISender sender, CancellationToken ct)
    {
        await sender.Send(new EditCourtCommand
        {
            CourtId = courtId,
            Name = request.Name,
            IsActive = request.IsActive,
            OverrideReservationDuration = request.OverrideReservationDuration,
            Images = request.Images,
            RemovedImageUrls = request.RemovedImageUrls
        }, ct);

        return Results.NoContent();
    }

    private static async Task<IResult> GetFacilityCourts([FromRoute] string facilitySlug, [AsParameters] GetFacilityCourtsQuery query, ISender sender, CancellationToken ct)
    {
        query.FacilitySlug = facilitySlug;
        var result = await sender.Send(query, ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveCourt(Guid facilityId, Guid courtId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new RemoveCourtCommand(courtId), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> GetCourtBySlug(string facilitySlug, string courtSlug, ISender sender, CancellationToken ct)
    {
        var court = await sender.Send(new GetFacilityCourtByIdQuery(facilitySlug, courtSlug), ct);

        if (court is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(court);
    }
}
