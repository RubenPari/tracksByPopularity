using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Service implementation for playlist-related operations.
/// Handles playlist management including track removal and creation of specialized playlists.
/// </summary>
public class PlaylistService : IPlaylistService
{
    /// <summary>
    /// Removes all tracks from a specified playlist by calling an external service endpoint.
    /// </summary>
    /// <param name="playlistId">The unique identifier of the playlist to clear.</param>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A <see cref="RemoveAllTracksResponse"/> indicating the operation result.
    /// </returns>
    /// <remarks>
    /// Uses the Spotify SDK to replace all playlist items with an empty list,
    /// effectively clearing the playlist.
    /// </remarks>
    public async Task<RemoveAllTracksResponse> RemoveAllTracksAsync(string playlistId, SpotifyClient spotifyClient)
    {
        try
        {
            await spotifyClient.Playlists.ReplaceItems(playlistId, new PlaylistReplaceItemsRequest(new List<string>()));
            return RemoveAllTracksResponse.Success;
        }
        catch (APIUnauthorizedException)
        {
            return RemoveAllTracksResponse.Unauthorized;
        }
        catch (APIException)
        {
            return RemoveAllTracksResponse.BadRequest;
        }
    }

    /// <summary>
    /// Retrieves all playlists owned by the current user.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A list of <see cref="PlaylistInfo"/> objects representing all user playlists.
    /// </returns>
    public async Task<IList<PlaylistInfo>> GetAllUserPlaylistsAsync(SpotifyClient spotifyClient)
    {
        var userId = (await spotifyClient.UserProfile.Current()).Id;
        var playlistsFirstPage = await spotifyClient.Playlists.GetUsers(userId);
        var allPlaylists = await spotifyClient.PaginateAll(playlistsFirstPage);

        var mapper = new PlaylistMapper();
        return allPlaylists.Select(p => mapper.MapToPlaylistInfo(p)).ToList();
    }
}
