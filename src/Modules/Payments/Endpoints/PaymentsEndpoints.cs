using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Payments.Models;
using Reservations.Shared;
using Reservations.Shared.DTOs;
using Stripe;
using Stripe.Checkout;

namespace Payments.Endpoints;

public sealed class PaymentEndpoints : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/payments").WithTags("Payments");

        group.MapPost("/create-checkout-session", CreateCheckoutSession)
            .WithName("CreateCheckoutSession")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
    }

    private static async Task<IResult> CreateCheckoutSession(
        CheckoutRequest request,
        IConfiguration configuration,
        IReservationsModuleApi reservationModuleApi)
    {
        StripeConfiguration.ApiKey = configuration["stripe:secret_key"];

        var reservation = await reservationModuleApi.GetByIdAsync(request.ReservationId);

        if (reservation is null)
        {
            return Results.BadRequest("Nieprawidłowy identyfikator rezerwacji.");
        }
        var priceInCents = (int)(reservation.Price * 100);

        var options = new SessionCreateOptions
        {
            PaymentMethodTypes = ["card"],
            LineItems =
            [
                new()
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "pln",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = GenerateReservationName(reservation),
                        },
                        UnitAmount = priceInCents,
                    },
                    Quantity = 1,
                },
            ],
            Mode = "payment",
            Metadata = new Dictionary<string, string>
            {
                { "reservationId", request.ReservationId.ToString() },
            },
            SuccessUrl = $"{request.SuccessUrl}?session_id={{CHECKOUT_SESSION_ID}}",
            CancelUrl = request.CancelUrl,
        };

        var service = new SessionService();
        var session = await service.CreateAsync(options);

        return Results.Ok(new { url = session.Url });
    }

    private static string GenerateReservationName(ReservationDetailsDto reservation)
    {
        return $"Rezerwacja kortu {reservation.CourtId} na {reservation.StartTime:yyyy-MM-dd HH:mm}";
    }
}
