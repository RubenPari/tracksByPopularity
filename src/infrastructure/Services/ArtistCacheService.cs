using SpotifyAPI.Web;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Service for caching followed artists.
/// Follows SRP: Only responsible for artist caching logic.
/// Uses DIP: Depends on ICacheRepository abstraction.
/// </summary>
public class ArtistCacheService : IArtistCacheService
{
    private readonly ICacheRepository _cache;
    private const string KeyPrefix = "artists:";
    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(30);

    public ArtistCacheService(ICacheRepository cache)
    {
        _cache = cache;
    }

    public async Task<ISet<string>> GetFollowedArtistsAsync(SpotifyClient spotifyClient, string spotifyUserId)
    {
        var key = $"{KeyPrefix}{spotifyUserId}";
        
        // Try to get from cache first
        var cached = await _cache.GetAsync<List<string>>(key);
        if (cached != null)
        {
            return new HashSet<string>(cached);
        }

        // Cache miss: fetch from API
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
        
        // Store in cache with longer TTL (artists change less frequently)
        await _cache.SetAsync(key, followedIds.ToList(), CacheTtl);
        
        return followedIds;
    }

    public async Task InvalidateAsync(string spotifyUserId)
    {
        var key = $"{KeyPrefix}{spotifyUserId}";
        await _cache.RemoveAsync(key);
    }
}
