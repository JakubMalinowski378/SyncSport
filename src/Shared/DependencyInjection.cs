using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedFramework(this IServiceCollection services)
    {
        services.AddCarter();
        services.AddSwaggerGen();

        return services;
    }

    public static WebApplication UseSharedFramework(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapCarter();

        return app;
    }
}
