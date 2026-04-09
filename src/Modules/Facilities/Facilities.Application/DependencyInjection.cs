using Facilities.Application.Abstractions;
using Facilities.Application.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Behaviors;

namespace Facilities.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFacilitiesApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddScoped<IFacilityAuthorizationService, FacilityAuthorizationService>();

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(UserContextBehavior<,>));
        });

        return services;
    }
}
