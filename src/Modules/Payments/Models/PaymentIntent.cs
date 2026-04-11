namespace Payments.Models;

public record PaymentIntent(decimal Amount, string? ReferenceId = null)
{
    public string Currency => "PLN";
}