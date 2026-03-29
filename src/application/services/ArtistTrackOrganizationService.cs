using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Application service implementation for organizing artist tracks into playlists.
/// Orchestrates the business logic of categorizing artist tracks by popularity.
/// </summary>
public class ArtistTrackOrganizationService : IArtistTrackOrganizationService
{
    private readonly ITrackCategorizationService _categorizationService;
    private readonly ITrackService _trackService;
    private readonly IPlaylistHelper _playlistHelper;
    private readonly IPlaylistService _playlistService;
    private readonly IPlaylistBackupService _backupService;
    private readonly ILogger<ArtistTrackOrganizationService> _logger;

    public ArtistTrackOrganizationService(
        ITrackCategorizationService categorizationService,
        ITrackService trackService,
        IPlaylistHelper playlistHelper,
        IPlaylistService playlistService,
        IPlaylistBackupService backupService,
        ILogger<ArtistTrackOrganizationService> logger
    )
    {
        _categorizationService = categorizationService;
        _trackService = trackService;
        _playlistHelper = playlistHelper;
        _playlistService = playlistService;
        _backupService = backupService;
        _logger = logger;
    }

    public async Task<bool> OrganizeArtistTracksAsync(
        string spotifyUserId,
        IList<SavedTrack> allTracks,
        string artistId,
        SpotifyClient spotifyClient
    )
    {
        _logger.LogInformation("Organizing tracks for artist: {ArtistId}", artistId);

        var artistPlaylists = await _playlistHelper.GetOrCreateArtistPlaylistsAsync(
            spotifyClient,
            artistId
        );

        foreach (var (category, playlistId) in artistPlaylists)
        {
            _logger.LogInformation("Snapshotting and clearing playlist {PlaylistId} for category {Category}", playlistId, category);
            await _backupService.CreateSnapshotAsync(spotifyUserId, playlistId, spotifyClient, "artist");
            var cleared = await _playlistService.RemoveAllTracksAsync(playlistId, spotifyClient);

            if (cleared == RemoveAllTracksResponse.Success) continue;

            _logger.LogWarning("Failed to clear playlist {PlaylistId} for category {Category}", playlistId, category);
            return false;
        }

        var domainTracks = SpotifyTrackMapper.ToDomain(allTracks);
        var categorizedTracks = _categorizationService.CategorizeArtistTracks(
            domainTracks,
            artistId
        );

        var results = new List<bool>();

        foreach (var (category, playlistId) in artistPlaylists)
        {
            if (!categorizedTracks.TryGetValue(category, out var tracks) || !tracks.Any())
            {
                _logger.LogInformation("No tracks found for category {Category}", category);
                results.Add(true);
                continue;
            }

            var tracksToAdd = allTracks.Where(savedTrack =>
                tracks.Any(dt => dt.Id == savedTrack.Track.Id)
            ).ToList();

            _logger.LogInformation(
                "Adding {Count} tracks to playlist {PlaylistId} for category {Category}",
                tracksToAdd.Count,
                playlistId,
                category
            );

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                playlistId,
                tracksToAdd
            );

            results.Add(added);

            if (!added)
            {
                _logger.LogWarning("Failed to add tracks to playlist {PlaylistId} for category {Category}", playlistId, category);
            }
        }

        var allSucceeded = results.All(r => r);

        if (allSucceeded)
        {
            _logger.LogInformation("Successfully organized all tracks for artist: {ArtistId}", artistId);
        }

        return allSucceeded;
    }
}
