using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedFramework(this IServiceCollection services)
    {
        services.AddCarter();

        return services;
    }

    public static WebApplication UseSharedFramework(this WebApplication app)
    {
        app.MapCarter();

        return app;
    }
}
