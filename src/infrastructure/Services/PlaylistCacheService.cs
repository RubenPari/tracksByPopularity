using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Service for caching user playlists.
/// Follows SRP: Only responsible for playlist caching logic.
/// Uses DIP: Depends on ICacheRepository abstraction.
/// </summary>
public class PlaylistCacheService : IPlaylistCacheService
{
    private readonly ICacheRepository _cache;
    private readonly IPlaylistService _playlistService;
    private const string KeyPrefix = "playlists:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(15);

    public PlaylistCacheService(ICacheRepository cache, IPlaylistService playlistService)
    {
        _cache = cache;
        _playlistService = playlistService;
    }

    public async Task<IList<PlaylistInfo>> GetPlaylistsAsync(SpotifyClient spotifyClient, string spotifyUserId)
    {
        var key = $"{KeyPrefix}{spotifyUserId}";
        
        // Try to get from cache first
        var cached = await _cache.GetAsync<List<PlaylistInfo>>(key);
        if (cached != null)
        {
            return cached;
        }

        // Cache miss: fetch from API
        var playlists = await _playlistService.GetAllUserPlaylistsAsync(spotifyClient);
        
        // Store in cache
        await _cache.SetAsync(key, playlists.ToList(), CacheTtl);
        
        return playlists;
    }

    public async Task InvalidateAsync(string spotifyUserId)
    {
        var key = $"{KeyPrefix}{spotifyUserId}";
        await _cache.RemoveAsync(key);
    }
}
