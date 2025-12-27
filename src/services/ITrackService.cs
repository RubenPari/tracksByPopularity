using SpotifyAPI.Web;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for track-related operations.
/// Provides methods to retrieve user tracks and manage playlist tracks.
/// </summary>
public interface ITrackService
{
    /// <summary>
    /// Retrieves all tracks from the user's Spotify library.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <returns>
    /// A list of all saved tracks from the user's library.
    /// The method handles pagination automatically to retrieve all tracks.
    /// </returns>
    Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(SpotifyClient spotifyClient);

    /// <summary>
    /// Adds a collection of tracks to a specified Spotify playlist.
    /// The method handles pagination automatically, adding tracks in batches of 100.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="playlistId">The unique identifier of the target playlist.</param>
    /// <param name="tracks">The collection of tracks to add to the playlist.</param>
    /// <returns>
    /// <c>true</c> if all tracks were successfully added; otherwise, <c>false</c>.
    /// Returns <c>false</c> if any batch fails to add.
    /// </returns>
    Task<bool> AddTracksToPlaylistAsync(
        SpotifyClient spotifyClient,
        string playlistId,
        IList<SavedTrack> tracks
    );
}

