using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Application.Interfaces;

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
    /// <param name="spotifyUserId">The Spotify user ID for user-specific cache keys.</param>
    /// <returns>
    /// A list of all saved tracks from the user's library.
    /// If cached data exists, it is returned immediately; otherwise, tracks are
    /// fetched from Spotify API and cached for future requests.
    /// </returns>
    /// <remarks>
    /// This method implements a cache-aside pattern:
    /// 1. First checks Redis cache for existing track data using user-specific key
    /// 2. If cache miss, fetches from Spotify API via TrackService
    /// 3. Stores the fetched data in Redis cache for subsequent requests
    /// Cache key format: "tracks:{spotifyUserId}"
    /// Default TTL: 10 minutes
    /// </remarks>
    Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient,
        string spotifyUserId
    );

    /// <summary>
    /// Retrieves all playlists owned by the user with caching support.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="spotifyUserId">The Spotify user ID for user-specific cache keys.</param>
    /// <returns>
    /// A list of all user playlists.
    /// If cached data exists, it is returned immediately; otherwise, playlists are
    /// fetched from Spotify API and cached for future requests.
    /// </returns>
    /// <remarks>
    /// Cache key format: "playlists:{spotifyUserId}"
    /// Default TTL: 15 minutes
    /// </remarks>
    Task<IList<PlaylistInfo>> GetUserPlaylistsWithCacheAsync(
        SpotifyClient spotifyClient,
        string spotifyUserId
    );

    /// <summary>
    /// Retrieves the set of followed artist IDs with caching support.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="spotifyUserId">The Spotify user ID for user-specific cache keys.</param>
    /// <returns>
    /// A set of followed artist IDs.
    /// If cached data exists, it is returned immediately; otherwise, artists are
    /// fetched from Spotify API and cached for future requests.
    /// </returns>
    /// <remarks>
    /// Cache key format: "artists:{spotifyUserId}"
    /// Default TTL: 30 minutes (artist follows change less frequently)
    /// </remarks>
    Task<ISet<string>> GetFollowedArtistsWithCacheAsync(
        SpotifyClient spotifyClient,
        string spotifyUserId
    );

    /// <summary>
    /// Invalidates all cache entries for a specific user.
    /// Call this when user data needs to be refreshed immediately.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID whose cache should be invalidated.</param>
    Task InvalidateUserCacheAsync(string spotifyUserId);

    /// <summary>
    /// Invalidates the tracks cache for a specific user.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID.</param>
    Task InvalidateTracksCacheAsync(string spotifyUserId);

    /// <summary>
    /// Invalidates the playlists cache for a specific user.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID.</param>
    Task InvalidatePlaylistsCacheAsync(string spotifyUserId);
}

