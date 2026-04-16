using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservations.Domain.Services;
using Reservations.Infrastructure.Persistence;
using Reservations.Infrastructure.Services;
using Shared.Persistence;
using Shared.Persistence.Interceptors;

namespace Reservations.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddReservationsInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is missing.");

        services.AddDbContext<ReservationsDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>());
        });

        services.AddScoped<IRepository<Reservations.Domain.Entities.Reservation, Guid>, ReservationsRepository<Reservations.Domain.Entities.Reservation, Guid>>();
        services.AddScoped<IReservationChecker, ReservationChecker>();

        return services;
    }
}
