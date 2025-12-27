using SpotifyAPI.Web;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for playlist helper operations.
/// Provides methods to manage artist-specific playlists.
/// </summary>
public interface IPlaylistHelper
{
    /// <summary>
    /// Retrieves or creates three playlists for a specific artist, categorized by popularity:
    /// "less" (low popularity), "medium" (medium popularity), and "more" (high popularity).
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="artistId">The unique identifier of the artist.</param>
    /// <returns>
    /// A dictionary containing playlist IDs with keys:
    /// - "less": Playlist ID for low popularity tracks
    /// - "medium": Playlist ID for medium popularity tracks
    /// - "more": Playlist ID for high popularity tracks
    /// </returns>
    /// <remarks>
    /// This method first searches for existing playlists with the naming pattern
    /// "{artistName} less", "{artistName} medium", "{artistName} more".
    /// If any of these playlists don't exist, they are created automatically.
    /// </remarks>
    Task<Dictionary<string, string>> GetOrCreateArtistPlaylistsAsync(
        SpotifyClient spotifyClient,
        string artistId
    );
}

