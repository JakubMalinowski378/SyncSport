using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Behaviors;
using Users.Shared.Authorization;

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
