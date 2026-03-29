using Microsoft.Extensions.Logging;

namespace Shared.Seeding;

internal sealed class DataSeederRunner(IEnumerable<IDataSeeder> seeders, ILogger<DataSeederRunner> logger)
{
    public async Task RunSeedersAsync(CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting data seeding process. Found {Count} seeders.", seeders.Count());

        foreach (var seeder in seeders)
        {
            var seederName = seeder.GetType().Name;
            logger.LogInformation("Running seeder: {SeederName}", seederName);
            
            try
            {
                await seeder.SeedAsync(cancellationToken);
                logger.LogInformation("Successfully completed seeder: {SeederName}", seederName);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred while running seeder: {SeederName}", seederName);
                throw;
            }
        }

        logger.LogInformation("Data seeding process completed.");
    }
}