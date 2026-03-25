using tracksByPopularity.Domain.Services;
using tracksByPopularity.Domain.ValueObjects;
using tracksByPopularity.Application.Mapping;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Application.Services;
using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Application service implementation for organizing tracks into playlists.
/// Orchestrates the business logic of categorizing and adding tracks to playlists.
/// </summary>
public class TrackOrganizationService : ITrackOrganizationService
{
    private readonly ITrackCategorizationService _categorizationService;
    private readonly ITrackService _trackService;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaylistBackupService _backupService;
    private readonly IPlaylistHelper _playlistHelper;
    private readonly ILogger<TrackOrganizationService> _logger;

    public TrackOrganizationService(
        ITrackCategorizationService categorizationService,
        ITrackService trackService,
        IPlaylistService playlistService,
        IPlaylistBackupService backupService,
        IPlaylistHelper playlistHelper,
        ILogger<TrackOrganizationService> logger
    )
    {
        _categorizationService = categorizationService;
        _trackService = trackService;
        _playlistService = playlistService;
        _backupService = backupService;
        _playlistHelper = playlistHelper;
        _logger = logger;
    }

    public async Task<bool> OrganizeTracksByPopularityAsync(
        IList<SavedTrack> allTracks,
        PopularityRange popularityRange,
        SpotifyClient spotifyClient
    )
    {
        // Get or create the system-managed playlist for this popularity range
        var playlistId = await _playlistHelper.GetOrCreatePopularityPlaylistAsync(spotifyClient, popularityRange);

        _logger.LogInformation(
            "Organizing tracks by popularity range {Min}-{Max} for playlist {PlaylistId}",
            popularityRange.Min,
            popularityRange.Max,
            playlistId
        );

        // Snapshot before clearing
        await _backupService.CreateSnapshotAsync(playlistId, spotifyClient, "popularity");

        // Clear existing tracks first
        await _playlistService.RemoveAllTracksAsync(playlistId, spotifyClient);

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

