using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Interface for playlist caching operations.
/// Follows ISP: Only methods related to playlists are exposed.
/// </summary>
public interface IPlaylistCacheService
{
    /// <summary>
    /// Retrieves all playlists owned by the user with caching support.
    /// </summary>
    Task<IList<PlaylistInfo>> GetPlaylistsAsync(SpotifyClient spotifyClient, string spotifyUserId);

    /// <summary>
    /// Invalidates the playlists cache for a specific user.
    /// </summary>
    Task InvalidateAsync(string spotifyUserId);
}
