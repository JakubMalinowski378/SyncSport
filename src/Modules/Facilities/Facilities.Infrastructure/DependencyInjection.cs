using Facilities.Infrastructure.Persistence;
using Facilities.Infrastructure.Services;
using Facilities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence;
using Shared.Persistence.Interceptors;

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

        services.AddScoped<IRepository<Facilities.Domain.Entities.Facility, Facilities.Domain.ValueObjects.FacilityId>, FacilitiesRepository<Facilities.Domain.Entities.Facility, Facilities.Domain.ValueObjects.FacilityId>>();
        services.AddScoped<IRepository<Facilities.Domain.Entities.Court, Facilities.Domain.ValueObjects.CourtId>, FacilitiesRepository<Facilities.Domain.Entities.Court, Facilities.Domain.ValueObjects.CourtId>>();
        services.AddScoped<IFacilitiesModuleApi, FacilitiesModuleApi>();

        return services;
    }
}
