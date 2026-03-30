using Facilities.Infrastructure.Persistence;
using Facilities.Infrastructure.Seeding;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence;
using Shared.Seeding;

namespace Facilities.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is missing.");

        services.AddDbContext<FacilitiesDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<,>), typeof(FacilitiesRepository<,>));
        return services;
    }
}
