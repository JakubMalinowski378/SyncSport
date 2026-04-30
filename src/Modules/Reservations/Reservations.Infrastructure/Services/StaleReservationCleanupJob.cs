using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Reservations.Shared;

namespace Reservations.Infrastructure.Services;

internal sealed class StaleReservationCleanupJob(IServiceScopeFactory scopeFactory, ILogger<StaleReservationCleanupJob> logger) : BackgroundService
{
    private static readonly TimeSpan _interval = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan _staleThreshold = TimeSpan.FromMinutes(15);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("StaleReservationCleanupJob started");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await Task.Delay(_interval, stoppingToken);

                using var scope = scopeFactory.CreateScope();
                var moduleApi = scope.ServiceProvider.GetRequiredService<IReservationsModuleApi>();

                var cancelledCount = await moduleApi.CancelStalePendingAsync(_staleThreshold, stoppingToken);

                if (cancelledCount > 0)
                {
                    logger.LogInformation("Cancelled {Count} stale pending reservations", cancelledCount);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error while cleaning up stale reservations");
            }
        }

        logger.LogInformation("StaleReservationCleanupJob stopped");
    }
}
