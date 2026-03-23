using System.Net;
using SpotifyAPI.Web;
using tracksByPopularity.Domain.Enums;
using tracksByPopularity.Infrastructure.Helpers;
using tracksByPopularity.Application.Mapping;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Service implementation for playlist-related operations.
/// Handles playlist management including track removal and creation of specialized playlists.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public PlaylistService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Removes all tracks from a specified playlist by calling an external service endpoint.
    /// </summary>
    /// <param name="playlistId">The unique identifier of the playlist to clear.</param>
    /// <returns>
    /// A <see cref="RemoveAllTracksResponse"/> indicating the operation result.
    /// </returns>
    /// <remarks>
    /// This method calls an external service at http://localhost:3000/playlist/delete-tracks
    /// to perform the track removal operation. The timeout is set to 200 seconds to handle
    /// large playlists that may take time to process.
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

    /// <summary>
    /// Legacy static method for backward compatibility.
    /// </summary>
    /// <param name="playlistId">The unique identifier of the playlist to clear.</param>
    /// <returns>A <see cref="RemoveAllTracksResponse"/> indicating the operation result.</returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="IPlaylistService.RemoveAllTracksAsync"/> instead
    /// through dependency injection for better testability and resource management.
    /// </remarks>
    [Obsolete("Use IPlaylistService.RemoveAllTracksAsync instead")]
    public static async Task<RemoveAllTracksResponse> RemoveAllTracks(string playlistId)
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(200);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"http://localhost:3000/playlist/delete-tracks?id={playlistId}"),
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => RemoveAllTracksResponse.Unauthorized,
            HttpStatusCode.BadRequest => RemoveAllTracksResponse.BadRequest,
            _ => RemoveAllTracksResponse.Success,
        };
    }
}
