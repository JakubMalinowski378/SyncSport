using System.Text;
using Carter;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Shared.Authorization;
using Shared.Exceptions;
using Shared.Persistence.Interceptors;
using Shared.Seeding;

namespace Shared;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedFramework(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        var jwtSection = configuration.GetSection("Jwt");
        var issuer = jwtSection["Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer is missing.");
        var audience = jwtSection["Audience"] ?? throw new InvalidOperationException("Jwt:Audience is missing.");
        var secret = jwtSection["Secret"] ?? throw new InvalidOperationException("Jwt:Secret is missing.");

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = issuer,
                    ValidateAudience = true,
                    ValidAudience = audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret))
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.Admin, policy => policy.RequireRole(Domain.Enums.UserRole.Admin.ToString()));
            options.AddPolicy(Policies.Manager, policy => policy.RequireRole(Domain.Enums.UserRole.Manager.ToString()));
            options.AddPolicy(Policies.User, policy => policy.RequireRole(Domain.Enums.UserRole.User.ToString()));
            options.AddPolicy(Policies.AdminOrManager, policy => policy.RequireRole(
                Domain.Enums.UserRole.Admin.ToString(),
                Domain.Enums.UserRole.Manager.ToString()));
        });

        services.AddCors(options =>
        {
            var frontendUrl = configuration["Frontend:Url"];
            if (!string.IsNullOrEmpty(frontendUrl))
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.WithOrigins(frontendUrl)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            }
        });

        services.AddCarter();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document, null)] = []
            });
        });

        services.AddTransient<DataSeederRunner>();
        services.AddScoped<PublishDomainEventsInterceptor>();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails();

        return services;
    }

    public static WebApplication UseSharedFramework(this WebApplication app)
    {
        app.UseExceptionHandler();

        app.UseCors();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options => options.DisplayOperationId());
        }

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapCarter();

        return app;
    }

    public static async Task SeedDataAsync(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            using var scope = app.Services.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<DataSeederRunner>();
            await runner.RunSeedersAsync();
        }
    }
}
