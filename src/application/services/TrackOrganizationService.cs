using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Application service implementation for organizing tracks into playlists.
/// Orchestrates the business logic of categorizing and adding tracks to playlists.
/// </summary>
public class TrackOrganizationService(
    ITrackCategorizationService categorizationService,
    ITrackService trackService,
    IPlaylistService playlistService,
    IPlaylistBackupService backupService,
    IPlaylistHelper playlistHelper,
    ILogger<TrackOrganizationService> logger)
    : ITrackOrganizationService
{
    /// <summary>
    /// Organizes tracks into playlists based on their popularity.
    /// </summary>
    public async Task<bool> OrganizeTracksByPopularityAsync(
        string spotifyUserId,
        IList<SavedTrack> allTracks,
        PopularityRange popularityRange,
        SpotifyClient spotifyClient
    )
    {
        // Get or create the system-managed playlist for this popularity range
        var playlistId = await playlistHelper.GetOrCreatePopularityPlaylistAsync(spotifyClient, popularityRange);

        logger.LogInformation(
            "Organizing tracks by popularity range {Min}-{Max} for playlist {PlaylistId}",
            popularityRange.Min,
            popularityRange.Max,
            playlistId
        );

        // Snapshot before clearing
        await backupService.CreateSnapshotAsync(spotifyUserId, playlistId, spotifyClient, "popularity");

        // Clear existing tracks first
        await playlistService.RemoveAllTracksAsync(playlistId, spotifyClient);

        // Convert to domain entities
        var domainTracks = SpotifyTrackMapper.ToDomain(allTracks);

        // Categorize using domain service (business logic)
        var categorizedTracks = categorizationService.CategorizeByPopularity(
            domainTracks,
            popularityRange
        ).ToList();

        if (categorizedTracks.Count == 0)
        {
            logger.LogInformation("No tracks found in popularity range {Min}-{Max}", popularityRange.Min, popularityRange.Max);
            return true; // No tracks to add is not an error
        }

        // Convert back to infrastructure models for API call
        var tracksToAdd = allTracks.Where(savedTrack =>
            categorizedTracks.Any(dt => dt.Id == savedTrack.Track.Id)
        ).ToList();

        logger.LogInformation("Adding {Count} tracks to playlist {PlaylistId}", tracksToAdd.Count, playlistId);

        // Add tracks via infrastructure service
        var result = await trackService.AddTracksToPlaylistAsync(
            spotifyClient,
            playlistId,
            tracksToAdd
        );

        if (result)
        {
            logger.LogInformation("Successfully added {Count} tracks to playlist {PlaylistId}", tracksToAdd.Count, playlistId);
        }
        else
        {
            logger.LogWarning("Failed to add tracks to playlist {PlaylistId}", playlistId);
        }

        return result;
    }
}

