using Payments.Enums;
using Payments.Models;

namespace Payments.Interfaces;

public interface IPaymentGateway
{
    Task<PaymentResult> InitializePaymentAsync(PaymentIntent intent);
    Task<PaymentStatus> GetStatusAsync(string externalTransactionId);
}