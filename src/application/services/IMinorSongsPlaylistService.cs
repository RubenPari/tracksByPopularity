namespace tracksByPopularity.application.services;

/// <summary>
/// Application service for creating and managing the MinorSongs playlist.
/// Orchestrates the business logic of creating playlists with tracks from lesser-known artists.
/// </summary>
public interface IMinorSongsPlaylistService
{
    /// <summary>
    /// Creates or updates a "MinorSongs" playlist containing tracks from artists
    /// that have 5 or fewer songs in the user's library.
    /// </summary>
    /// <param name="allTracks">All user tracks to filter.</param>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>
    /// <c>true</c> if the playlist was created/updated successfully; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> CreateOrUpdateMinorSongsPlaylistAsync(
        IList<SpotifyAPI.Web.SavedTrack> allTracks,
        SpotifyAPI.Web.SpotifyClient spotifyClient
    );
}

