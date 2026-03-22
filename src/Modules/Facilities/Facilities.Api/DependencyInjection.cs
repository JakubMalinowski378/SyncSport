using Facilities.Application;
using Facilities.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facilities.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddFacilitiesModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddFacilitiesApplication();
        services.AddInfrastructure(configuration);

        return services;
    }
}
