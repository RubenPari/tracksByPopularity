using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Service interface for playlist-related operations.
/// Provides methods to manage playlists and their tracks.
/// </summary>
public interface IPlaylistService
{
    /// <summary>
    /// Removes all tracks from a specified playlist.
    /// This operation is performed via an external service endpoint.
    /// </summary>
    /// <param name="playlistId">The unique identifier of the playlist to clear.</param>
    /// <returns>
    /// A <see cref="RemoveAllTracksResponse"/> indicating the result of the operation:
    /// - <see cref="RemoveAllTracksResponse.Success"/> if tracks were removed successfully
    /// - <see cref="RemoveAllTracksResponse.Unauthorized"/> if authentication failed
    /// - <see cref="RemoveAllTracksResponse.BadRequest"/> if the request was invalid
    /// </returns>
    Task<RemoveAllTracksResponse> RemoveAllTracksAsync(string playlistId, SpotifyClient spotifyClient);

    /// <summary>
    /// Retrieves all playlists owned by the current user.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A list of <see cref="PlaylistInfo"/> objects representing all user playlists.
    /// </returns>
    Task<IList<PlaylistInfo>> GetAllUserPlaylistsAsync(SpotifyClient spotifyClient);
}

