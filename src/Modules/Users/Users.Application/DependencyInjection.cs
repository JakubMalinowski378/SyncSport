using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Shared.Behaviors;

namespace Users.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddMediatR(configuration =>
        {
            configuration.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            configuration.AddOpenBehavior(typeof(ValidationBehavior<,>));
        });

        services.AddScoped<Shared.IUsersModuleApi, Services.UsersModuleApi>();
        
        return services;
    }
}