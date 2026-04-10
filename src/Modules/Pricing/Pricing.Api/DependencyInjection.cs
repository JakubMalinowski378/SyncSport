using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Application;
using Pricing.Infrastructure;

namespace Pricing.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPricingModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddPricingApplication();
        services.AddPricingInfrastructure(configuration);

        return services;
    }
}