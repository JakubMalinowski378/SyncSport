namespace Payments.Models;

public sealed class CheckoutRequest
{
    public Guid ReservationId { get; set; }
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}