using tracksByPopularity.domain.services;
using tracksByPopularity.domain.valueobjects;
using tracksByPopularity.infrastructure.mappers;
using tracksByPopularity.services;
using SpotifyAPI.Web;

namespace tracksByPopularity.application.services;

/// <summary>
/// Application service implementation for organizing tracks into playlists.
/// Orchestrates the business logic of categorizing and adding tracks to playlists.
/// </summary>
public class TrackOrganizationService : ITrackOrganizationService
{
    private readonly ITrackCategorizationService _categorizationService;
    private readonly ITrackService _trackService;
    private readonly ILogger<TrackOrganizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackOrganizationService"/> class.
    /// </summary>
    /// <param name="categorizationService">Domain service for categorizing tracks.</param>
    /// <param name="trackService">Service for adding tracks to playlists.</param>
    /// <param name="logger">Logger instance for recording operations.</param>
    public TrackOrganizationService(
        ITrackCategorizationService categorizationService,
        ITrackService trackService,
        ILogger<TrackOrganizationService> logger
    )
    {
        _categorizationService = categorizationService;
        _trackService = trackService;
        _logger = logger;
    }

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
    /// <remarks>
    /// This method:
    /// 1. Converts infrastructure models (SavedTrack) to domain entities (Track)
    /// 2. Uses domain service to categorize tracks by popularity
    /// 3. Converts back to infrastructure models for API call
    /// 4. Adds tracks to playlist via infrastructure service
    /// </remarks>
    public async Task<bool> OrganizeTracksByPopularityAsync(
        IList<SavedTrack> allTracks,
        PopularityRange popularityRange,
        string playlistId,
        SpotifyClient spotifyClient
    )
    {
        _logger.LogInformation(
            "Organizing tracks by popularity range {Min}-{Max} for playlist {PlaylistId}",
            popularityRange.Min,
            popularityRange.Max,
            playlistId
        );

        // Convert to domain entities
        var domainTracks = SpotifyTrackMapper.ToDomain(allTracks);

        // Categorize using domain service (business logic)
        var categorizedTracks = _categorizationService.CategorizeByPopularity(
            domainTracks,
            popularityRange
        ).ToList();

        if (!categorizedTracks.Any())
        {
            _logger.LogInformation("No tracks found in popularity range {Min}-{Max}", popularityRange.Min, popularityRange.Max);
            return true; // No tracks to add is not an error
        }

        // Convert back to infrastructure models for API call
        var tracksToAdd = allTracks.Where(savedTrack =>
            categorizedTracks.Any(dt => dt.Id == savedTrack.Track.Id)
        ).ToList();

        _logger.LogInformation("Adding {Count} tracks to playlist {PlaylistId}", tracksToAdd.Count, playlistId);

        // Add tracks via infrastructure service
        var result = await _trackService.AddTracksToPlaylistAsync(
            spotifyClient,
            playlistId,
            tracksToAdd
        );

        if (result)
        {
            _logger.LogInformation("Successfully added {Count} tracks to playlist {PlaylistId}", tracksToAdd.Count, playlistId);
        }
        else
        {
            _logger.LogWarning("Failed to add tracks to playlist {PlaylistId}", playlistId);
        }

        return result;
    }
}

