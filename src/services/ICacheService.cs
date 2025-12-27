using SpotifyAPI.Web;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for cache-related operations.
/// Provides methods to retrieve user tracks with caching support.
/// </summary>
public interface ICacheService
{
    /// <summary>
    /// Retrieves all tracks from the user's Spotify library, using Redis cache
    /// to improve performance by avoiding redundant API calls.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A list of all saved tracks from the user's library.
    /// If cached data exists, it is returned immediately; otherwise, tracks are
    /// fetched from Spotify API and cached for future requests.
    /// </returns>
    /// <remarks>
    /// This method implements a cache-aside pattern:
    /// 1. First checks Redis cache for existing track data
    /// 2. If cache miss, fetches from Spotify API via TrackService
    /// 3. Stores the fetched data in Redis cache for subsequent requests
    /// </remarks>
    Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient
    );
}

