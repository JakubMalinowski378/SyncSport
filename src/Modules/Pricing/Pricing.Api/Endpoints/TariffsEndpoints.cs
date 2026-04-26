using Carter;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Pricing.Application.Tariffs.Commands.CreateTariff;
using Pricing.Application.Tariffs.Commands.EditTariff;
using Pricing.Application.Tariffs.DTOs;
using Pricing.Application.Tariffs.Queries.CalculatePrice;
using Pricing.Application.Tariffs.Queries.GetFacilityTariffs;
using Shared.Authorization;

namespace Pricing.Api.Endpoints;

public sealed class TariffsEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/tariffs").WithTags("Tariffs");

        group.MapPost("/", CreateTariff)
            .WithName("CreateTariff")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapPut("/facility/{facilityId:guid}", EditTariff)
            .WithName("EditTariff")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization(Policies.AdminOrManager);

        group.MapGet("/facility/{facilityId:guid}", GetFacilityTariffs)
            .WithName("GetFacilityTariffs")
            .Produces<IEnumerable<TariffDto>>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/calculate", CalculatePrice)
            .WithName("CalculatePrice")
            .Produces<decimal>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateTariff(
        [FromBody] CreateTariffCommand command,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var tariffId = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/tariffs/{tariffId}", tariffId);
    }

    private static async Task<IResult> EditTariff(
        [FromRoute] Guid facilityId,
        [FromBody] EditTariffRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var wasUpdated = await sender.Send(
            new EditTariffCommand(facilityId, request.BaseHourlyRate, request.CourtOverrides),
            cancellationToken);

        return wasUpdated ? Results.NoContent() : Results.NotFound();
    }

    private static async Task<IResult> GetFacilityTariffs(
        [FromRoute] Guid facilityId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var query = new GetFacilityTariffsQuery(facilityId);
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }

    private sealed record EditTariffRequest(
        decimal BaseHourlyRate,
        IReadOnlyCollection<EditCourtRateOverrideRequest>? CourtOverrides
    );

    private static async Task<IResult> CalculatePrice(
        [FromBody] CalculatePriceQuery query,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(query, cancellationToken);
        return Results.Ok(result);
    }
}