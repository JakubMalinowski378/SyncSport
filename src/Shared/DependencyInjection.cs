using Carter;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shared.Authorization;
using Shared.Persistence.Interceptors;
using Shared.Seeding;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedFramework(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddAuthentication();
        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.Admin, policy => policy.RequireRole(Domain.Enums.UserRole.Admin.ToString()));
            options.AddPolicy(Policies.Manager, policy => policy.RequireRole(Domain.Enums.UserRole.Manager.ToString()));
            options.AddPolicy(Policies.User, policy => policy.RequireRole(Domain.Enums.UserRole.User.ToString()));
        });

        services.AddCarter();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddTransient<DataSeederRunner>();
        services.AddScoped<PublishDomainEventsInterceptor>();

        return services;
    }

    public static WebApplication UseSharedFramework(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI();

        app.UseAuthentication();
        app.UseAuthorization();

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
