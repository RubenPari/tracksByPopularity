namespace tracksByPopularity.application.services;

/// <summary>
/// Application service for organizing artist tracks into playlists.
/// Orchestrates the business logic of categorizing artist tracks by popularity.
/// </summary>
public interface IArtistTrackOrganizationService
{
    /// <summary>
    /// Organizes tracks from a specific artist into three playlists based on popularity.
    /// </summary>
    /// <param name="allTracks">All user tracks to filter.</param>
    /// <param name="artistId">The unique identifier of the artist.</param>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>
    /// <c>true</c> if all tracks were successfully organized; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> OrganizeArtistTracksAsync(
        IList<SpotifyAPI.Web.SavedTrack> allTracks,
        string artistId,
        SpotifyAPI.Web.SpotifyClient spotifyClient
    );
}

