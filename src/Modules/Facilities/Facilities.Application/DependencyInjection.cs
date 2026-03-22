using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Facilities.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddFacilitiesApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        return services;
    }
}
