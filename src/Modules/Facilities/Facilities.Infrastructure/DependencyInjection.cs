using Facilities.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Facilities.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("FacilitiesDatabase")
            ?? throw new InvalidOperationException("Connection string 'FacilitiesDatabase' is missing.");

        services.AddDbContext<FacilitiesDbContext>(options =>
            options.UseNpgsql(connectionString));

        return services;
    }
}
