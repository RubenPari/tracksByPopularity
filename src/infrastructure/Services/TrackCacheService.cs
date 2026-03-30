using SpotifyAPI.Web;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Service for caching user tracks.
/// Follows SRP: Only responsible for track caching logic.
/// Uses DIP: Depends on ICacheRepository abstraction.
/// </summary>
public class TrackCacheService : ITrackCacheService
{
    private readonly ICacheRepository _cache;
    private readonly ITrackService _trackService;
    private const string KeyPrefix = "tracks:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    public TrackCacheService(ICacheRepository cache, ITrackService trackService)
    {
        _cache = cache;
        _trackService = trackService;
    }

    public async Task<IList<SavedTrack>> GetTracksAsync(SpotifyClient spotifyClient, string spotifyUserId)
    {
        var key = $"{KeyPrefix}{spotifyUserId}";
        
        // Try to get from cache first
        var cached = await _cache.GetAsync<List<SavedTrack>>(key);
        if (cached != null)
        {
            return cached;
        }

        // Cache miss: fetch from API
        var tracks = await _trackService.GetAllUserTracksWithClientAsync(spotifyClient);
        
        // Store in cache
        await _cache.SetAsync(key, tracks.ToList(), CacheTtl);
        
        return tracks;
    }

    public async Task InvalidateAsync(string spotifyUserId)
    {
        var key = $"{KeyPrefix}{spotifyUserId}";
        await _cache.RemoveAsync(key);
    }
}
