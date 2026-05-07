using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Storage;

public static class Extensions
{
    public static IServiceCollection AddImageStorage(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<BlobStorageOptions>(configuration.GetSection("BlobStorage"));
        services.AddScoped<IImageStorageService, BlobStorageService>();

        return services;
    }
}