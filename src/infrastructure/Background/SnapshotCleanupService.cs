using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Background;

public class SnapshotCleanupService(
    IPlaylistBackupService backupService,
    ILogger<SnapshotCleanupService> logger)
    : BackgroundService
{
    private static readonly TimeSpan CleanupInterval = TimeSpan.FromHours(24);
    private const int SnapshotRetentionDays = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Snapshot cleanup service started. Retention: {Days} days", SnapshotRetentionDays);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var deletedCount = await backupService.DeleteOldSnapshotsAsync(SnapshotRetentionDays);
                if (deletedCount > 0)
                {
                    logger.LogInformation("Cleaned up {Count} old snapshots", deletedCount);
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during snapshot cleanup");
            }

            await Task.Delay(CleanupInterval, stoppingToken);
        }
    }
}
