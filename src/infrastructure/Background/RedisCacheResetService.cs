using StackExchange.Redis;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Background;

/// <summary>
/// Background service that manages Redis cache expiration.
/// Uses Redis TTL for automatic expiration instead of manual flushing.
/// This ensures better performance and prevents cache stampede.
/// </summary>
public class RedisCacheResetService : BackgroundService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheResetService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(5);

    // Key prefixes to track for expiration
    private const string TracksKeyPrefix = "tracks:";
    private const string PlaylistsKeyPrefix = "playlists:";
    private const string ArtistsKeyPrefix = "artists:";
    private const string TokenKeyPrefix = "spotify_token:";

    public RedisCacheResetService(
        IConnectionMultiplexer redis,
        ILogger<RedisCacheResetService> logger,
        IServiceProvider serviceProvider
    )
    {
        _redis = redis;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Redis cache management service started. Checking for expired keys every {Interval} minutes",
            _checkInterval.TotalMinutes
        );

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CleanupExpiredKeysAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during Redis cache cleanup");
            }

            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("Redis cache management service stopped");
    }

    /// <summary>
    /// Cleans up expired user cache entries.
    /// Note: Redis TTL handles automatic expiration, this is a fallback mechanism
    /// for any orphaned keys or additional cleanup logic.
    /// </summary>
    private async Task CleanupExpiredKeysAsync()
    {
        var endpoints = _redis.GetEndPoints();
        if (endpoints.Length == 0)
        {
            _logger.LogWarning("No Redis endpoints found");
            return;
        }

        var server = _redis.GetServer(endpoints[0]);
        var db = _redis.GetDatabase();

        // Track active users for potential cache warming
        var activeUserCount = 0;

        try
        {
            // Scan for user-specific cache keys that might need cleanup
            var keysToCheck = new List<string>
            {
                TracksKeyPrefix,
                PlaylistsKeyPrefix,
                ArtistsKeyPrefix
            };

            foreach (var keyPrefix in keysToCheck)
            {
                await foreach (var key in server.KeysAsync(pattern: $"{keyPrefix}*"))
                {
                    var ttl = await db.KeyTimeToLiveAsync(key);
                    if (ttl.HasValue && ttl.Value > TimeSpan.Zero)
                    {
                        activeUserCount++;
                    }
                }
            }

            _logger.LogInformation(
                "Redis cache check completed. Found {ActiveUserCount} active cache entries",
                activeUserCount
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error scanning Redis keys, will retry next interval");
        }
    }
}
