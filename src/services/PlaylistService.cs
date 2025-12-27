using System.Net;
using SpotifyAPI.Web;
using tracksByPopularity.models;
using tracksByPopularity.utils;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for playlist-related operations.
/// Handles playlist management including track removal and creation of specialized playlists.
/// </summary>
public class PlaylistService : IPlaylistService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IArtistService _artistService;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients for external service calls.</param>
    /// <param name="artistService">Service for retrieving artist summary information.</param>
    public PlaylistService(IHttpClientFactory httpClientFactory, IArtistService artistService)
    {
        _httpClientFactory = httpClientFactory;
        _artistService = artistService;
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
    public async Task<RemoveAllTracksResponse> RemoveAllTracksAsync(string playlistId)
    {
        var client = _httpClientFactory.CreateClient();
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

    /// <summary>
    /// Creates or updates a "MinorSongs" playlist containing tracks from artists
    /// that have 5 or fewer songs in the user's library.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="tracks">The complete collection of user's saved tracks.</param>
    /// <returns>
    /// <c>true</c> if the playlist was created/updated successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method performs the following steps:
    /// 1. Retrieves the current user's ID
    /// 2. Checks if a "MinorSongs" playlist exists, creating it if necessary
    /// 3. Retrieves artist summary to identify artists with ≤5 songs
    /// 4. Filters tracks to include only those from qualifying artists
    /// 5. Adds filtered tracks to the playlist in paginated batches of 100
    /// </remarks>
    public async Task<bool> CreatePlaylistTracksMinorAsync(
        SpotifyClient spotifyClient,
        IList<SavedTrack> tracks
    )
    {
        var userId = (await spotifyClient.UserProfile.Current()).Id;

        // Check if "MinorSongs" playlist already exists
        var playlistsUserFirstPage = await spotifyClient.Playlists.GetUsers(userId);
        var playlistsUser = await spotifyClient.PaginateAll(playlistsUserFirstPage);

        var idPlaylistMinorSongs = playlistsUser
            .FirstOrDefault(playlist => playlist.Name == Constants.PlaylistNameWithMinorSongs)
            ?.Id;

        if (idPlaylistMinorSongs == null)
        {
            // Create playlist "MinorSongs" if it doesn't exist
            var playlistMinorSongs = await spotifyClient.Playlists.Create(
                userId,
                new PlaylistCreateRequest(Constants.PlaylistNameWithMinorSongs)
            );

            idPlaylistMinorSongs = playlistMinorSongs.Id;
        }

        // Get artists with less than 5 songs in user library
        var artistsSummary = await _artistService.GetArtistsSummaryAsync();

        if (artistsSummary == null)
        {
            return false;
        }

        // Find all tracks that belong to artists with less than 5 songs
        var tracksToKeep = artistsSummary
            .Where(artistSummary => artistSummary.Count <= 5)
            .SelectMany(artistSummary =>
                tracks.Where(track => track.Track.Artists[0].Id == artistSummary.Id)
            )
            .ToList();

        // Convert tracks to Uris for playlist addition
        var tracksUris = TrackUtils.ConvertTracksToUris(tracksToKeep);

        // Insert tracks into playlist with pagination (100 tracks per batch)
        var offset = Constants.Offset;
        var limit = Constants.LimitInsertPlaylistTracks;

        while (true)
        {
            var tracksToAdd = tracksUris.Skip(offset).Take(limit).ToList();

            var added = await spotifyClient.Playlists.AddItems(
                idPlaylistMinorSongs!,
                new PlaylistAddItemsRequest(tracksToAdd)
            );

            // If the operation fails, Spotify returns an empty SnapshotId
            if (added.SnapshotId == string.Empty)
            {
                return false;
            }

            // If we've added fewer tracks than the limit, we've reached the end
            if (tracksToAdd.Count < limit)
            {
                break;
            }

            offset += limit;
        }

        return true;
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
