using SpotifyAPI.Web;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for track-related operations.
/// Handles retrieval of user tracks and adding tracks to playlists.
/// </summary>
public class TrackService : ITrackService
{
    /// <summary>
    /// Retrieves all tracks from the user's Spotify library.
    /// Automatically handles pagination to fetch all tracks across multiple pages.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A complete list of all saved tracks from the user's library.
    /// </returns>
    /// <remarks>
    /// This method first retrieves the first page of tracks, then uses the
    /// Spotify API's pagination functionality to fetch all remaining pages.
    /// </remarks>
    public async Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient
    )
    {
        var firstPageTracks = await spotifyClient.Library.GetTracks();
        return await spotifyClient.PaginateAll(firstPageTracks);
    }

    /// <summary>
    /// Adds a collection of tracks to a specified Spotify playlist.
    /// Processes tracks in batches of 100 (Spotify API limit) to handle large collections.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="playlistId">The unique identifier of the target playlist.</param>
    /// <param name="tracks">The collection of tracks to add to the playlist.</param>
    /// <returns>
    /// <c>true</c> if all tracks were successfully added; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// The method processes tracks in batches of 100 due to Spotify API limitations.
    /// If any batch fails (indicated by an empty SnapshotId), the method returns false
    /// and stops processing remaining batches.
    /// </remarks>
    public async Task<bool> AddTracksToPlaylistAsync(
        SpotifyClient spotifyClient,
        string playlistId,
        IList<SavedTrack> tracks
    )
    {
        // Process tracks in batches of 100 (Spotify API limit per request)
        for (var i = 0; i < tracks.Count; i += 100)
        {
            var tracksToAdd = tracks.Skip(i).Take(100).Select(track => track.Track.Uri).ToList();

            var added = await spotifyClient.Playlists.AddItems(
                playlistId,
                new PlaylistAddItemsRequest(tracksToAdd)
            );

            // If the operation fails, Spotify returns an empty SnapshotId
            if (added.SnapshotId == string.Empty)
            {
                return false;
            }
        }

        return true;
    }
}
