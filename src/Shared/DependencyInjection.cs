using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Seeding;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedFramework(this IServiceCollection services)
    {
        services.AddCarter();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddTransient<DataSeederRunner>();

        return services;
    }

    public static WebApplication UseSharedFramework(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        
        app.MapCarter();

        return app;
    }

    public static async Task SeedDataAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<DataSeederRunner>();
            await runner.RunSeedersAsync();
        }
    }
}
