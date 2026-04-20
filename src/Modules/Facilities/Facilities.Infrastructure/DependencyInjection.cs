using Facilities.Domain.Entities;
using Facilities.Domain.ValueObjects;
using Facilities.Infrastructure.Persistence;
using Facilities.Infrastructure.Seeding;
using Facilities.Infrastructure.Services;
using Facilities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence;
using Shared.Persistence.Interceptors;
using Shared.Seeding;

namespace Facilities.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is missing.");

        services.AddDbContext<FacilitiesDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>());
        });

        services.AddScoped<IRepository<Facility, FacilityId>, FacilitiesRepository<Facility, FacilityId>>();
        services.AddScoped<IRepository<Court, CourtId>, FacilitiesRepository<Court, CourtId>>();
        services.AddScoped<IFacilitiesModuleApi, FacilitiesModuleApi>();

        services.AddScoped<IDataSeeder, FacilitySeeder>();

        return services;
    }
}
