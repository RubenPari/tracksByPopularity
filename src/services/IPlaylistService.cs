using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

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
    Task<RemoveAllTracksResponse> RemoveAllTracksAsync(string playlistId);

    /// <summary>
    /// Creates or updates a playlist with tracks from artists that have 5 or fewer songs
    /// in the user's library. This playlist is named "MinorSongs".
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="tracks">The collection of all user tracks to filter.</param>
    /// <returns>
    /// <c>true</c> if the playlist was created/updated successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method:
    /// 1. Checks if a "MinorSongs" playlist exists, creating it if necessary
    /// 2. Retrieves artist summary data to identify artists with ≤5 songs
    /// 3. Filters tracks to include only those from qualifying artists
    /// 4. Adds filtered tracks to the playlist in paginated batches
    /// </remarks>
    Task<bool> CreatePlaylistTracksMinorAsync(
        SpotifyClient spotifyClient,
        IList<SavedTrack> tracks
    );
}

