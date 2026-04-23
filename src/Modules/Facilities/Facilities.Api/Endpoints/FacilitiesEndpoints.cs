using System.Security.Claims;
using System.Text.Json;
using Carter;
using Facilities.Application.Facilities.Commands.CreateCourt;
using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Application.Facilities.Commands.EditCourt;
using Facilities.Application.Facilities.Commands.EditFacility;
using Facilities.Application.Facilities.Commands.GetAllFacilities;
using Facilities.Application.Facilities.Commands.GetFacilityById;
using Facilities.Application.Facilities.Commands.RemoveCourt;
using Facilities.Application.Facilities.Commands.RemoveFacility;
using Facilities.Application.Facilities.Queries.GetFacilityCourts;
using Facilities.Application.Facilities.Queries.GetFacilityCourtById;
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
            .DisableAntiforgery();

        group.MapGet("/", GetFacilities)
            .WithName("GetFacilities")
            .Produces<PagedResult<GetAllFacilitiesResult>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/{facilityId:guid}", GetFacilityById)
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

        group.MapGet("/{facilityId:guid}/courts/{courtId:guid}", GetCourtById)
            .WithName("GetCourtById")
            .Produces<CourtDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

        group.MapGet("/{facilityId:guid}/courts", GetFacilityCourts)
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

    private static async Task<(List<string>? Urls, IResult? Error)> ProcessImagesAsync(IFormFileCollection? images, Storage.IImageStorageService imageStorageService, CancellationToken ct)
    {
        var imageUrls = new List<string>();
        if (images is null || !images.Any())
        {
            return (imageUrls, null);
        }

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        var filesToUpload = new List<(Stream Stream, string ContentType, string FileName)>();

        foreach (var file in images)
        {
            var extension = System.IO.Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
            {
                return (null, Results.BadRequest(new { Error = $"Invalid image format for file '{file.FileName}'. Allowed extensions are: {string.Join(", ", allowedExtensions)}" }));
            }

            var newFileName = $"{Guid.NewGuid()}{extension}";
            filesToUpload.Add((file.OpenReadStream(), file.ContentType, newFileName));
        }

        imageUrls = (await imageStorageService.AddRangeAsync(filesToUpload, ct)).ToList();
        return (imageUrls, null);
    }

    private static async Task<IResult> CreateFacility([FromForm] CreateFacilityRequest request, ISender sender, Storage.IImageStorageService imageStorageService, CancellationToken ct)
    {
        var (imageUrls, error) = await ProcessImagesAsync(request.Images, imageStorageService, ct);
        if (error is not null)
        {
            return error;
        }

        var jsonSerializer = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var weeklyHours = !string.IsNullOrWhiteSpace(request.WeeklyHours)
            ? JsonSerializer.Deserialize<List<DailyHoursDto>>(request.WeeklyHours, jsonSerializer)
            : null;

        var customDateHours = !string.IsNullOrWhiteSpace(request.CustomDateHours)
            ? JsonSerializer.Deserialize<List<DateSpecificHoursDto>>(request.CustomDateHours, jsonSerializer)
            : null;

        var id = await sender.Send(new CreateFacilityCommand(request.Name, request.Address, request.ReservationDuration, weeklyHours, customDateHours, imageUrls), ct);

        return Results.Created($"/api/facilities/{id}", id);
    }

    private static async Task<IResult> GetFacilities([AsParameters] GetAllFacilitiesCommand query, ISender sender, CancellationToken ct)
    {
        var facilities = await sender.Send(query, ct);

        return Results.Ok(facilities);
    }

    private static async Task<IResult> GetFacilityById(Guid facilityId, ISender sender, CancellationToken ct)
    {
        var facility = await sender.Send(new GetFacilityByIdCommand(facilityId), ct);

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

    private static async Task<IResult> EditFacility([FromRoute] Guid facilityId, [FromForm] EditFacilityRequest request, ISender sender, Storage.IImageStorageService imageStorageService, CancellationToken ct)
    {
        var (imageUrls, error) = await ProcessImagesAsync(request.Images, imageStorageService, ct);
        if (error is not null)
        {
            return error;
        }

        var weeklyHours = !string.IsNullOrWhiteSpace(request.WeeklyHours)
            ? System.Text.Json.JsonSerializer.Deserialize<List<DailyHoursDto>>(request.WeeklyHours, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            : null;

        var customDateHours = !string.IsNullOrWhiteSpace(request.CustomDateHours)
            ? System.Text.Json.JsonSerializer.Deserialize<List<DateSpecificHoursDto>>(request.CustomDateHours, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            : null;

        await sender.Send(new EditFacilityCommand(facilityId, request.Name, request.Address, request.ReservationDuration, weeklyHours, customDateHours, imageUrls), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> CreateCourt([FromRoute] Guid facilityId, [FromForm] CreateCourtRequest request, ISender sender, Storage.IImageStorageService imageStorageService, CancellationToken ct)
    {
        var (imageUrls, error) = await ProcessImagesAsync(request.Images, imageStorageService, ct);
        if (error is not null)
        {
            return error;
        }

        var courtId = await sender.Send(new CreateCourtCommand(facilityId, request.Name, request.SurfaceType, request.OverrideReservationDuration, imageUrls), ct);

        return Results.Created($"/api/facilities/{facilityId}/courts/{courtId}", courtId);
    }

    private static async Task<IResult> EditCourt([FromRoute] Guid facilityId, [FromRoute] Guid courtId, [FromForm] EditCourtRequest request, ISender sender, Storage.IImageStorageService imageStorageService, CancellationToken ct)
    {
        var (imageUrls, error) = await ProcessImagesAsync(request.Images, imageStorageService, ct);
        if (error is not null)
        {
            return error;
        }

        await sender.Send(new EditCourtCommand(facilityId, courtId, request.Name, request.IsActive, request.OverrideReservationDuration, imageUrls), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> GetFacilityCourts([AsParameters] GetFacilityCourtsQuery query, ISender sender, CancellationToken ct)
    {
        var result = await sender.Send(query, ct);

        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveCourt(Guid facilityId, Guid courtId, ISender sender, CancellationToken ct)
    {
        await sender.Send(new RemoveCourtCommand(facilityId, courtId), ct);

        return Results.NoContent();
    }

    private static async Task<IResult> GetCourtById(Guid facilityId, Guid courtId, ISender sender, CancellationToken ct)
    {
        var court = await sender.Send(new GetFacilityCourtByIdQuery(facilityId, courtId), ct);

        if (court is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(court);
    }

}

public sealed class CreateFacilityRequest
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int ReservationDuration { get; init; }
    public string? WeeklyHours { get; init; }
    public string? CustomDateHours { get; init; }
    public IFormFileCollection? Images { get; init; }
}

public sealed class EditFacilityRequest
{
    public string Name { get; init; } = string.Empty;
    public string Address { get; init; } = string.Empty;
    public int ReservationDuration { get; init; }
    public string? WeeklyHours { get; init; }
    public string? CustomDateHours { get; init; }
    public IFormFileCollection? Images { get; init; }
}

public sealed class CreateCourtRequest
{
    public string Name { get; init; } = string.Empty;
    public string SurfaceType { get; init; } = string.Empty;
    public int? OverrideReservationDuration { get; init; }
    public IFormFileCollection? Images { get; init; }
}

public sealed class EditCourtRequest
{
    public string Name { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public int? OverrideReservationDuration { get; init; }
    public IFormFileCollection? Images { get; init; }
}
