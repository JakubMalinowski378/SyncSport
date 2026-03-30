using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Shared.Persistence;

public static class DatabaseMigrator
{
    public static async Task ApplyDatabaseMigrationsAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<DbContext>>();

        var dbContextTypes = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(x => x.GetTypes())
            .Where(t => typeof(DbContext).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract && t != typeof(DbContext))
            .ToList();

        foreach (var dbContextType in dbContextTypes)
        {
            var dbContext = scope.ServiceProvider.GetService(dbContextType) as DbContext;
            if (dbContext is not null)
            {
                logger.LogInformation("Applying migrations for {DbContextName}...", dbContextType.Name);

                try
                {
                    await dbContext.Database.MigrateAsync();
                    logger.LogInformation("Migrations applied successfully for {DbContextName}.", dbContextType.Name);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "An error occurred while applying migrations for {DbContextName}.", dbContextType.Name);
                    throw;
                }
            }
        }
    }
}