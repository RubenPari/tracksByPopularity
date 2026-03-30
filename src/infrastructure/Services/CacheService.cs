using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Service implementation for cache-related operations.
/// Handles caching of user data in Redis to improve performance and reduce API calls.
/// </summary>
public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _cacheRedisConnection;
    private readonly ITrackService _trackService;
    private readonly IPlaylistService _playlistService;

    // Cache TTLs
    private static readonly TimeSpan TracksCacheTtl = TimeSpan.FromMinutes(10);
    private static readonly TimeSpan PlaylistsCacheTtl = TimeSpan.FromMinutes(15);
    private static readonly TimeSpan ArtistsCacheTtl = TimeSpan.FromMinutes(30);

    // Cache key prefixes
    private const string TracksKeyPrefix = "tracks:";
    private const string PlaylistsKeyPrefix = "playlists:";
    private const string ArtistsKeyPrefix = "artists:";

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheService"/> class.
    /// </summary>
    /// <param name="cacheRedisConnection">The Redis connection multiplexer for cache operations.</param>
    /// <param name="trackService">The track service for fetching tracks when cache misses occur.</param>
    /// <param name="playlistService">The playlist service for fetching playlists when cache misses occur.</param>
    public CacheService(
        IConnectionMultiplexer cacheRedisConnection,
        ITrackService trackService,
        IPlaylistService playlistService
    )
    {
        _cacheRedisConnection = cacheRedisConnection;
        _trackService = trackService;
        _playlistService = playlistService;
    }

    /// <inheritdoc />
    public async Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient,
        string spotifyUserId
    )
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();
        var cacheKey = $"{TracksKeyPrefix}{spotifyUserId}";

        // Check cache first
        var tracksCache = await cacheRedis.StringGetAsync(cacheKey);

        if (tracksCache.HasValue)
        {
            // Cache hit: deserialize and return cached data
            return JsonConvert.DeserializeObject<IList<SavedTrack>>(tracksCache!)!;
        }

        // Cache miss: fetch from Spotify API
        var allTracks = await _trackService.GetAllUserTracksWithClientAsync(spotifyClient);

        // Store in cache for future requests
        await cacheRedis.StringSetAsync(
            cacheKey,
            JsonConvert.SerializeObject(allTracks),
            TracksCacheTtl
        );

        return allTracks;
    }

    /// <inheritdoc />
    public async Task<IList<PlaylistInfo>> GetUserPlaylistsWithCacheAsync(
        SpotifyClient spotifyClient,
        string spotifyUserId
    )
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();
        var cacheKey = $"{PlaylistsKeyPrefix}{spotifyUserId}";

        // Check cache first
        var playlistsCache = await cacheRedis.StringGetAsync(cacheKey);

        if (playlistsCache.HasValue)
        {
            // Cache hit: deserialize and return cached data
            return JsonConvert.DeserializeObject<IList<PlaylistInfo>>(playlistsCache!)!;
        }

        // Cache miss: fetch from Spotify API
        var playlists = await _playlistService.GetAllUserPlaylistsAsync(spotifyClient);

        // Store in cache for future requests
        await cacheRedis.StringSetAsync(
            cacheKey,
            JsonConvert.SerializeObject(playlists),
            PlaylistsCacheTtl
        );

        return playlists;
    }

    /// <inheritdoc />
    public async Task<ISet<string>> GetFollowedArtistsWithCacheAsync(
        SpotifyClient spotifyClient,
        string spotifyUserId
    )
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();
        var cacheKey = $"{ArtistsKeyPrefix}{spotifyUserId}";

        // Check cache first
        var artistsCache = await cacheRedis.StringGetAsync(cacheKey);

        if (artistsCache.HasValue)
        {
            // Cache hit: deserialize and return cached data
            var artistIds = JsonConvert.DeserializeObject<List<string>>(artistsCache!)!;
            return new HashSet<string>(artistIds);
        }

        // Cache miss: fetch from Spotify API
        var followedIds = new HashSet<string>();
        string? after = null;

        while (true)
        {
            var followedRequest = new FollowOfCurrentUserRequest { Limit = 50 };
            if (after != null) followedRequest.After = after;

            var response = await spotifyClient.Follow.OfCurrentUser(followedRequest);
            var page = response.Artists;

            foreach (var artist in page.Items!)
            {
                followedIds.Add(artist.Id);
            }

            if (page.Cursors?.After == null) break;
            after = page.Cursors.After;
        }

        // Store in cache for future requests (use longer TTL for artists as they change less frequently)
        await cacheRedis.StringSetAsync(
            cacheKey,
            JsonConvert.SerializeObject(followedIds.ToList()),
            ArtistsCacheTtl
        );

        return followedIds;
    }

    /// <inheritdoc />
    public async Task InvalidateUserCacheAsync(string spotifyUserId)
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();
        var batch = cacheRedis.CreateBatch();

        var tracksKey = $"{TracksKeyPrefix}{spotifyUserId}";
        var playlistsKey = $"{PlaylistsKeyPrefix}{spotifyUserId}";
        var artistsKey = $"{ArtistsKeyPrefix}{spotifyUserId}";

        // Execute deletion in parallel
        var tasks = new[]
        {
            cacheRedis.KeyDeleteAsync(tracksKey),
            cacheRedis.KeyDeleteAsync(playlistsKey),
            cacheRedis.KeyDeleteAsync(artistsKey)
        };

        await Task.WhenAll(tasks);
    }

    /// <inheritdoc />
    public async Task InvalidateTracksCacheAsync(string spotifyUserId)
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();
        await cacheRedis.KeyDeleteAsync($"{TracksKeyPrefix}{spotifyUserId}");
    }

    /// <inheritdoc />
    public async Task InvalidatePlaylistsCacheAsync(string spotifyUserId)
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();
        await cacheRedis.KeyDeleteAsync($"{PlaylistsKeyPrefix}{spotifyUserId}");
    }
}
