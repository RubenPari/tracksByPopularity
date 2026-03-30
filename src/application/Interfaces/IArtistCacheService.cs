using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Interface for artist caching operations.
/// Follows ISP: Only methods related to artists are exposed.
/// </summary>
public interface IArtistCacheService
{
    /// <summary>
    /// Retrieves the set of followed artist IDs with caching support.
    /// </summary>
    Task<ISet<string>> GetFollowedArtistsAsync(SpotifyClient spotifyClient, string spotifyUserId);

    /// <summary>
    /// Invalidates the artists cache for a specific user.
    /// </summary>
    Task InvalidateAsync(string spotifyUserId);
}
