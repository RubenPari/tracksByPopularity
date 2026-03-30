using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Interface for track caching operations.
/// Follows ISP: Only methods related to tracks are exposed.
/// </summary>
public interface ITrackCacheService
{
    /// <summary>
    /// Retrieves all tracks from the user's Spotify library, using Redis cache.
    /// </summary>
    Task<IList<SavedTrack>> GetTracksAsync(SpotifyClient spotifyClient, string spotifyUserId);

    /// <summary>
    /// Invalidates the tracks cache for a specific user.
    /// </summary>
    Task InvalidateAsync(string spotifyUserId);
}
