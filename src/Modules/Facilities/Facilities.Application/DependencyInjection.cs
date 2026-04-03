using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Behaviors;

namespace Facilities.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFacilitiesApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
            configuration.AddOpenBehavior(typeof(UserContextBehavior<,>));
        });

        return services;
    }
}
