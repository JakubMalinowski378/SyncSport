using Microsoft.Extensions.Configuration;
using Payments.Enums;
using Payments.Interfaces;
using Payments.Models;

namespace Payments.Infrastructure;

public sealed class FakePaymentGateway(IConfiguration configuration) : IPaymentGateway
{
    public async Task<PaymentResult> InitializePaymentAsync(PaymentIntent intent)
    {
        await Task.Delay(500);

        // 999 to simulate failure
        if (intent.Amount == 999m)
        {
            return PaymentResult.Failed("Card declined");
        }

        var frontendUrl = configuration["Frontend:Url"]?.TrimEnd('/');

        return PaymentResult.Success(
            externalId: Guid.NewGuid().ToString(),
            redirectUrl: $"{frontendUrl}/mock-payment-page"
        );
    }

    public Task<PaymentStatus> GetStatusAsync(string externalTransactionId)
    {
        return Task.FromResult(PaymentStatus.Completed);
    }
}