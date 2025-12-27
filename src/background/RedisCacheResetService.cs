using StackExchange.Redis;

namespace tracksByPopularity.background;

public class RedisCacheResetService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheResetService> _logger;
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    public RedisCacheResetService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheResetService> logger
    )
    {
        _redis = redis;
        _logger = logger;
    }

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
