using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Reservations.Application.Abstractions;
using Reservations.Application.Services;
using Shared.Behaviors;

namespace Reservations.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddReservationsApplication(this IServiceCollection services)
    {
        services.AddScoped<IFacilityAuthorizationService, FacilityAuthorizationService>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        return services;
    }
}
