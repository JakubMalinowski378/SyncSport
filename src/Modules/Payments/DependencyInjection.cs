using Microsoft.Extensions.DependencyInjection;

namespace Payments;

public static class DependencyInjection
{
    public static IServiceCollection AddPaymentsModule(this IServiceCollection services)
    {
        return services;
    }
}