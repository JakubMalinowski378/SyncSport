using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Reservations.Application.Reservations.Queries.GetMyReservations;
using Reservations.Domain.Services;
using Reservations.Infrastructure.Persistence;
using Reservations.Infrastructure.Services;
using Reservations.Shared;
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

        services.AddScoped<ReservationRepository>();
        services.AddScoped<IRepository<Reservations.Domain.Entities.Reservation, Guid>>(sp => sp.GetRequiredService<ReservationRepository>());
        services.AddScoped<IReservationRepository>(sp => sp.GetRequiredService<ReservationRepository>());
        services.AddScoped<IReservationChecker, ReservationChecker>();
        services.AddScoped<IReservationsModuleApi, ReservationsModuleApi>();

        services.AddHostedService<StaleReservationCleanupJob>();

        return services;
    }
}
