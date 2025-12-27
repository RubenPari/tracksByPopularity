using StackExchange.Redis;

namespace tracksByPopularity.background;

/// <summary>
/// Background service that periodically resets the Redis cache to ensure data freshness.
/// Runs continuously and flushes the Redis database at regular intervals.
/// </summary>
public class RedisCacheResetService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheResetService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Initializes a new instance of the <see cref="RedisCacheResetService"/> class.
    /// </summary>
    /// <param name="redis">The Redis connection multiplexer for cache operations.</param>
    /// <param name="logger">The logger instance for recording service activities.</param>
    public RedisCacheResetService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheResetService> logger
    )
    {
        _redis = redis;
        _logger = logger;
    }

    /// <summary>
    /// Executes the background service task.
    /// Periodically flushes the Redis database to clear cached data.
    /// </summary>
    /// <param name="stoppingToken">Cancellation token to stop the service gracefully.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This service runs continuously, flushing the Redis cache every 5 minutes.
    /// This ensures that cached track data doesn't become stale and forces fresh
    /// data retrieval from the Spotify API when needed.
    /// 
    /// The service handles errors gracefully, logging them and continuing execution.
    /// </remarks>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Redis cache reset service started. Cache will be reset every {Interval} minutes",
            _interval.TotalMinutes
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var endpoints = _redis.GetEndPoints();
                if (endpoints.Length == 0)
                {
                    _logger.LogWarning("No Redis endpoints found");
                    await Task.Delay(_interval, stoppingToken);
                    continue;
                }

                var server = _redis.GetServer(endpoints[0]);
                await server.FlushDatabaseAsync();
                _logger.LogInformation("Redis cache flushed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting Redis cache");
            }

            await Task.Delay(_interval, stoppingToken);
        }

        _logger.LogInformation("Redis cache reset service stopped");
    }
}
