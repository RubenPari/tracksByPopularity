using SpotifyAPI.Web;
using tracksByPopularity.Domain.ValueObjects;

namespace tracksByPopularity.Application.Interfaces;

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

    /// <summary>
    /// Retrieves or creates the system-managed playlist for a specific popularity range.
    /// Playlists use predefined names like "Popularity: Less (0-20)".
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="popularityRange">The popularity range to get the playlist for.</param>
    /// <returns>The Spotify playlist ID for the given popularity range.</returns>
    Task<string> GetOrCreatePopularityPlaylistAsync(
        SpotifyClient spotifyClient,
        PopularityRange popularityRange
    );
}

