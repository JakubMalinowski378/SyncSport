using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Notifications;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationsModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpOptions>(configuration.GetSection("Smtp"));

        services.AddScoped<IEmailSender, EmailSender>();
        services.AddSingleton<ITemplateService, TemplateService>();

        return services;
    }
}
