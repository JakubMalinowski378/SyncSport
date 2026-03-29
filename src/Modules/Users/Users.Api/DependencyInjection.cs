using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Users.Application;
using Users.Infrastructure;

namespace Users.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddUsersApplication();
        services.AddUsersInfrastructure(configuration);

        return services;
    }
}