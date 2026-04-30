using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Reservations.Shared;
using Stripe;
using Stripe.Checkout;

namespace Payments.Endpoints;

public sealed class PaymentWebhook : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("webhooks/stripe/payments").WithTags("Webhooks");

        group.MapPost(string.Empty, HandleStripeWebhook)
            .WithName("HandleStripeWebhook")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .AllowAnonymous();
    }

    private static async Task<IResult> HandleStripeWebhook(
        HttpRequest request,
        IConfiguration configuration,
        IReservationsModuleApi reservationModuleApi,
        ILogger<PaymentWebhook> logger)
    {
        var secret = configuration["stripe:webhook_secret"];
        if (string.IsNullOrEmpty(secret))
        {
            logger.LogError("Stripe webhook secret is not configured");
            return Results.Problem("Webhook secret not configured", statusCode: StatusCodes.Status500InternalServerError);
        }

        string json;
        using (var reader = new StreamReader(request.Body))
        {
            json = await reader.ReadToEndAsync();
        }

        Event stripeEvent;
        try
        {
            stripeEvent = EventUtility.ConstructEvent(
                json,
                request.Headers["Stripe-Signature"],
                secret);
        }
        catch (StripeException ex)
        {
            logger.LogWarning(ex, "Invalid Stripe webhook signature");
            return Results.BadRequest("Invalid signature");
        }

        logger.LogInformation("Received Stripe webhook event: {EventType}", stripeEvent.Type);

        if (stripeEvent.Type == "checkout.session.completed")
        {
            await HandleCheckoutSessionCompleted(stripeEvent, reservationModuleApi, logger);
        }
        else
        {
            logger.LogInformation("Unhandled Stripe event type: {EventType}", stripeEvent.Type);
        }

        return Results.Ok();
    }

    private static async Task HandleCheckoutSessionCompleted(
        Event stripeEvent,
        IReservationsModuleApi reservationModuleApi,
        ILogger logger)
    {
        var session = stripeEvent.Data.Object as Session;
        if (session?.Metadata is null ||
            !session.Metadata.TryGetValue("reservationId", out var reservationIdString) ||
            !Guid.TryParse(reservationIdString, out var reservationId))
        {
            logger.LogWarning("Checkout session completed event missing valid reservationId in metadata");
            return;
        }

        var updated = await reservationModuleApi.UpdateStatusAsync(reservationId, "Paid");
        if (updated)
        {
            logger.LogInformation("Reservation {ReservationId} marked as Paid via webhook", reservationId);
        }
        else
        {
            logger.LogWarning("Failed to mark reservation {ReservationId} as Paid", reservationId);
        }
    }
}