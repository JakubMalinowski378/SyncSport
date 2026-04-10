using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pricing.Infrastructure.Persistence;
using Shared.Persistence;
using Shared.Persistence.Interceptors;

namespace Pricing.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPricingInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is missing.");

        services.AddDbContext<PricingDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>());
        });

        services.AddScoped(typeof(IRepository<,>), typeof(PricingRepository<,>));

        return services;
    }
}