using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.services;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for cache-related operations.
/// Handles caching of user tracks in Redis to improve performance and reduce API calls.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _cacheRedisConnection;
    private readonly ITrackService _trackService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="cacheRedisConnection">The Redis connection multiplexer for cache operations.</param>
    /// <param name="trackService">The track service for fetching tracks when cache misses occur.</param>
    public CacheService(
        IConnectionMultiplexer cacheRedisConnection,
        ITrackService trackService
    )
    {
        _cacheRedisConnection = cacheRedisConnection;
        _trackService = trackService;
    }

    /// <summary>
    /// Retrieves all tracks from the user's Spotify library with Redis caching support.
    /// Implements a cache-aside pattern to optimize performance.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A list of all saved tracks. Returns cached data if available, otherwise fetches
    /// from Spotify API and stores in cache.
    /// </returns>
    /// <remarks>
    /// Cache key: "allTracks"
    /// The cache is managed by a background service that periodically clears it
    /// to ensure data freshness (see RedisCacheResetService).
    /// </remarks>
    public async Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient
    )
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();

        // Check cache first
        var tracksCache = await cacheRedis.StringGetAsync("allTracks");

        IList<SavedTrack> allTracks;

        if (tracksCache.HasValue)
        {
            // Cache hit: deserialize and return cached data
            allTracks = JsonConvert.DeserializeObject<IList<SavedTrack>>(tracksCache!)!;
        }
        else
        {
            // Cache miss: fetch from Spotify API
            allTracks = await _trackService.GetAllUserTracksWithClientAsync(spotifyClient);

            // Store in cache for future requests
            await cacheRedis.StringSetAsync(
                "allTracks",
                JsonConvert.SerializeObject(allTracks)
            );
        }

        return allTracks;
    }
}

