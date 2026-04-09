using Carter;
using Facilities.Application.Facilities.Commands.CreateCourt;
using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Application.Facilities.Commands.EditFacility;
using Facilities.Application.Facilities.Commands.GetAllFacilities;
using Facilities.Application.Facilities.Commands.GetFacilityById;
using Facilities.Application.Facilities.Commands.RemoveCourt;
using Facilities.Application.Facilities.Commands.RemoveFacility;
using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Authorization;
using Shared.Pagination;
using System.Security.Claims;

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
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/", GetFacilities)
            .WithName("GetFacilities")
            .Produces<PagedResult<FacilityResponse>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{facilityId:guid}", GetFacilityById)
            .WithName("GetFacilityById")
            .Produces<FacilityResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapDelete("/{facilityId:guid}", RemoveFacility)
            .WithName("RemoveFacility")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPut("/{facilityId:guid}", EditFacility)
            .WithName("EditFacility")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapPost("/{facilityId:guid}/courts", CreateCourt)
            .WithName("CreateCourt")
            .Produces<Guid>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{facilityId:guid}/courts", GetFacilityCourts)
            .WithName("GetFacilityCourts")
            .Produces<PagedResult<CourtResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapDelete("/{facilityId:guid}/courts/{courtId:guid}", RemoveCourt)
            .WithName("RemoveCourt")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status403Forbidden)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization($"{Policies.Admin}, {Policies.Manager}");     
    }

    private static async Task<IResult> CreateFacility(CreateFacilityRequest request, ISender sender, CancellationToken ct)
    {
        var id = await sender.Send(
            new CreateFacilityCommand(request.Name, request.Address, request.OpenTime, request.CloseTime),
            ct);

        return Results.Created($"/api/facilities/{id}", id);
    }

    private static async Task<IResult> GetFacilities(int? pageNumber, int? pageSize, ISender sender, CancellationToken ct)
    {
        var facilities = await sender.Send(
            new GetAllFacilitiesCommand(pageNumber ?? 1, pageSize ?? 10),
            ct);

        var responseItems = facilities.Items
            .Select(x => new FacilityResponse(
                x.Id,
                x.Name,
                x.Address,
                x.OpeningHours))
            .ToList();

        var response = new PagedResult<FacilityResponse>(
            responseItems,
            facilities.TotalCount,
            facilities.PageNumber,
            facilities.PageSize);

        return Results.Ok(response);
    }

    private static async Task<IResult> GetFacilityById(Guid facilityId, ISender sender, CancellationToken ct)
    {
        var facility = await sender.Send(new GetFacilityByIdCommand(facilityId), ct);

        if (facility is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(new FacilityResponse(
            facility.Id,
            facility.Name,
            facility.Address,
            facility.OpeningHours));
    }

    private static async Task<IResult> RemoveFacility(Guid facilityId, ISender sender, CancellationToken ct)
    {
        var removed = await sender.Send(new RemoveFacilityCommand(facilityId), ct);

        if (!removed)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }

    private static async Task<IResult> EditFacility(Guid facilityId, EditFacilityRequest request, ISender sender, CancellationToken ct)
    {
        var edited = await sender.Send(
            new EditFacilityCommand(
                facilityId,
                request.Name,
                request.Address,
                request.OpenTime,
                request.CloseTime),
            ct);

        if (!edited)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }

    private static async Task<IResult> CreateCourt(Guid facilityId, CreateCourtRequest request, ISender sender, CancellationToken ct)
    {
        var courtId = await sender.Send(
            new CreateCourtCommand(facilityId, request.Name, request.SurfaceType),
            ct);

        return Results.Created($"/api/facilities/{facilityId}/courts/{courtId}", courtId);
    }

    private static async Task<IResult> GetFacilityCourts(Guid facilityId, int? pageNumber, int? pageSize, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(
            new GetFacilityCourtsQuery(facilityId, pageNumber ?? 1, pageSize ?? 10),
            ct);

        var responseItems = result.Items
            .Select(x => new CourtResponse(
                x.Id,
                x.Name,
                x.SurfaceType,
                x.IsActive))
            .ToList();

        var response = new PagedResult<CourtResponse>(
            responseItems,
            result.TotalCount,
            result.PageNumber,
            result.PageSize);

        return Results.Ok(response);
    }

    private static async Task<IResult> RemoveCourt(Guid facilityId, Guid courtId, ClaimsPrincipal user, ISender sender, CancellationToken ct)
    {
        var isAdmin = user.IsInRole("Admin");
        var isModerator = user.IsInRole("Moderator") || user.IsInRole("Manager");

        if (!isAdmin && !isModerator)
        {
            return Results.Forbid();
        }

        if (isModerator && !isAdmin)
        {
            var managedFacilityClaims = user.FindAll("ManagedFacilityId").Select(c => c.Value);
            if (!managedFacilityClaims.Contains(facilityId.ToString(), StringComparer.OrdinalIgnoreCase))
            {
                return Results.Forbid();
            }
        }

        var removed = await sender.Send(new RemoveCourtCommand(facilityId, courtId), ct);

        if (!removed)
        {
            return Results.NotFound();
        }

        return Results.NoContent();
    }

}

public sealed record CreateFacilityRequest(
    string Name,
    string Address,
    TimeSpan OpenTime,
    TimeSpan CloseTime);

public sealed record EditFacilityRequest(
    string Name,
    string Address,
    TimeSpan OpenTime,
    TimeSpan CloseTime);

public sealed record CreateCourtRequest(
    string Name,
    string SurfaceType);

public sealed record CourtResponse(
    Guid Id,
    string Name,
    string SurfaceType,
    bool IsActive);

public sealed record FacilityResponse(
    Guid Id,
    string Name,
    string Address,
    List<Facilities.Application.Facilities.Common.DailyOpeningHoursDto> OpeningHours);
