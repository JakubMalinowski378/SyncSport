using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Shared.Persistence;
using Users.Application.Abstractions;
using Users.Infrastructure.Authentication;
using Users.Infrastructure.Persistence;

namespace Users.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddUsersInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string is missing.");

        services.AddDbContext<UsersDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped(typeof(IRepository<,>), typeof(UsersRepository<,>));

        services.Configure<JwtOptions>(configuration.GetSection("Jwt"));
        services.AddTransient<IJwtService, JwtService>();
        services.AddTransient<IPasswordHasher, PasswordHasher>();

        services.Configure<PasswordHasherOptions>(configuration.GetSection("PasswordHasher"));
        return services;
    }
}