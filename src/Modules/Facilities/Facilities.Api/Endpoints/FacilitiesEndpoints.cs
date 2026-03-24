using Carter;
using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Shared.Persistence;

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
            .Produces<IReadOnlyCollection<FacilityResponse>>(StatusCodes.Status200OK);

        group.MapGet("/{facilityId:guid}", GetFacilityById)
            .WithName("GetFacilityById")
            .Produces<FacilityResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateFacility(CreateFacilityRequest request, ISender sender, CancellationToken ct)
    {
        try
        {
            var id = await sender.Send(
                new CreateFacilityCommand(request.Name, request.Address, request.OpenTime, request.CloseTime),
                ct);

            return Results.Created($"/api/facilities/{id}", id);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetFacilities(IRepository<Facility, FacilityId> facilityRepository, CancellationToken ct)
    {
        var facilities = await facilityRepository.GetAllAsync(asNoTracking: true, ct: ct);

        var response = facilities
            .Select(MapFacility)
            .ToList();

        return Results.Ok(response);
    }

    private static async Task<IResult> GetFacilityById(Guid facilityId, IRepository<Facility, FacilityId> facilityRepository, CancellationToken ct)
    {
        var facility = await facilityRepository.GetByIdAsync(
            new FacilityId(facilityId),
            asNoTracking: true,
            ct: ct);

        if (facility is null)
        {
            return Results.NotFound();
        }

        return Results.Ok(MapFacility(facility));
    }

    private static FacilityResponse MapFacility(Facility facility)
        => new(
            facility.Id.Value,
            facility.Name,
            facility.Address,
            facility.OpeningHours.OpenTime,
            facility.OpeningHours.CloseTime);
}

public sealed record CreateFacilityRequest(
    string Name,
    string Address,
    TimeSpan OpenTime,
    TimeSpan CloseTime);

public sealed record FacilityResponse(
    Guid Id,
    string Name,
    string Address,
    TimeSpan OpenTime,
    TimeSpan CloseTime);
