using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Notifications.Interfaces;
using Notifications.Options;
using Notifications.Services;
using Shared.Behaviors;

namespace Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));
        services.Configure<FrontendOptions>(configuration.GetSection("Frontend"));
        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<ITemplateService, TemplateService>();
        services.AddMediatR(config =>
        {
            config.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
            config.AddOpenBehavior(typeof(ValidationBehavior<,>));
            config.AddOpenBehavior(typeof(UserContextBehavior<,>));
        });
        return services;
    }
}
