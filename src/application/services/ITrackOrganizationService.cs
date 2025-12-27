using tracksByPopularity.domain.valueobjects;

namespace tracksByPopularity.application.services;

/// <summary>
/// Application service for organizing tracks into playlists.
/// Orchestrates the business logic of categorizing and adding tracks to playlists.
/// </summary>
public interface ITrackOrganizationService
{
    /// <summary>
    /// Organizes tracks by popularity range and adds them to the specified playlist.
    /// </summary>
    /// <param name="allTracks">All user tracks to categorize.</param>
    /// <param name="popularityRange">The popularity range to filter by.</param>
    /// <param name="playlistId">The playlist ID to add tracks to.</param>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>
    /// <c>true</c> if tracks were successfully added; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> OrganizeTracksByPopularityAsync(
        IList<SpotifyAPI.Web.SavedTrack> allTracks,
        PopularityRange popularityRange,
        string playlistId,
        SpotifyAPI.Web.SpotifyClient spotifyClient
    );
}

