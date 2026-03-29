namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Application service for organizing tracks into playlists.
/// Orchestrates the business logic of categorizing and adding tracks to playlists.
/// </summary>
public interface ITrackOrganizationService
{
    /// <summary>
    /// Organizes tracks by popularity range and adds them to the specified playlist.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID.</param>
    /// <param name="allTracks">All user tracks to categorize.</param>
    /// <param name="popularityRange">The popularity range to filter by.</param>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>
    /// <c>true</c> if tracks were successfully added; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> OrganizeTracksByPopularityAsync(
        string spotifyUserId,
        IList<SpotifyAPI.Web.SavedTrack> allTracks,
        PopularityRange popularityRange,
        SpotifyAPI.Web.SpotifyClient spotifyClient
    );
}

