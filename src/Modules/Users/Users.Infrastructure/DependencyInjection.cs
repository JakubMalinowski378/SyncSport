using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence;
using Shared.Persistence.Interceptors;
using Users.Application.Abstractions;
using Users.Infrastructure.Authentication;
using Users.Infrastructure.Persistence;
using Users.Shared;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is missing.");

        services.AddDbContext<UsersDbContext>((sp, options) =>
        {
            options.UseNpgsql(connectionString);
            options.AddInterceptors(sp.GetRequiredService<PublishDomainEventsInterceptor>());
        });

        services.AddScoped<IRepository<Users.Domain.Entities.User, Guid>, UsersRepository<Users.Domain.Entities.User, Guid>>();
        services.AddScoped<IRepository<Users.Domain.Entities.Account, Guid>, UsersRepository<Users.Domain.Entities.Account, Guid>>();

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddTransient<IJwtService, JwtService>();
        services.AddTransient<IPasswordHasher, PasswordHasher>();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.Configure<PasswordHasherOptions>(configuration.GetSection("PasswordHasher"));
        return services;
    }
}