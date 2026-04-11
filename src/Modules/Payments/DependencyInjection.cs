using Microsoft.Extensions.DependencyInjection;
using Payments.Infrastructure;
using Payments.Interfaces;

namespace Payments;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        services.AddScoped<IPaymentGateway, FakePaymentGateway>();

        return services;
    }
}