using Carter;
using Facilities.Application.Facilities.Commands.CreateFacility;
using Facilities.Application.Facilities.Commands.GetAllFacilities;
using Facilities.Application.Facilities.Commands.GetFacilityById;
using Facilities.Application.Facilities.Commands.RemoveFacility;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    private static async Task<IResult> GetFacilities(int? pageNumber, int? pageSize, ISender sender, CancellationToken ct)
    {
        try
        {
            var facilities = await sender.Send(
                new GetAllFacilitiesCommand(pageNumber ?? 1, pageSize ?? 10),
                ct);

            var responseItems = facilities.Items
                .Select(x => new FacilityResponse(
                    x.Id,
                    x.Name,
                    x.Address,
                    x.OpenTime,
                    x.CloseTime))
                .ToList();

            var response = new PagedResult<FacilityResponse>(
                responseItems,
                facilities.TotalCount,
                facilities.PageNumber,
                facilities.PageSize);

            return Results.Ok(response);
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> GetFacilityById(Guid facilityId, ISender sender, CancellationToken ct)
    {
        try
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
                facility.OpenTime,
                facility.CloseTime));
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

    private static async Task<IResult> RemoveFacility(Guid facilityId, ISender sender, CancellationToken ct)
    {
        try
        {
            var removed = await sender.Send(new RemoveFacilityCommand(facilityId), ct);

            if (!removed)
            {
                return Results.NotFound();
            }

            return Results.NoContent();
        }
        catch (ArgumentException ex)
        {
            return Results.BadRequest(new { message = ex.Message });
        }
    }

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
