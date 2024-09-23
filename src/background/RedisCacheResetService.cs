using StackExchange.Redis;

namespace tracksByPopularity.src.background;

public class RedisCacheResetService(IConnectionMultiplexer redis) : BackgroundService
{
    private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var endpoints = redis.GetEndPoints();
                var server = redis.GetServer(endpoints[0]);
                await server.FlushDatabaseAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error resetting Redis cache: {ex.Message}");
            }

            await Task.Delay(_interval, stoppingToken);
        }
    }
}
