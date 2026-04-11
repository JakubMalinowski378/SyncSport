namespace Payments.Models;

public record PaymentResult(bool IsSuccess, string? ExternalId, string? RedirectUrl, string? ErrorMessage)
{
    public static PaymentResult Success(string externalId, string redirectUrl)
        => new(true, externalId, redirectUrl, null);

    public static PaymentResult Failed(string errorMessage)
        => new(false, null, null, errorMessage);
}