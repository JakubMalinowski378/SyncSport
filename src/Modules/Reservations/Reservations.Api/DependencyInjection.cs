using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservations.Application;
using Reservations.Infrastructure;

namespace Reservations.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddReservationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddReservationsApplication();
        services.AddReservationsInfrastructure(configuration);
        return services;
    }
}
