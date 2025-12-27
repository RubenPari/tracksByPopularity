using tracksByPopularity.domain.services;
using tracksByPopularity.infrastructure.mappers;
using tracksByPopularity.services;
using SpotifyAPI.Web;

namespace tracksByPopularity.application.services;

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
    private readonly ILogger<ArtistTrackOrganizationService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistTrackOrganizationService"/> class.
    /// </summary>
    /// <param name="categorizationService">Domain service for categorizing tracks.</param>
    /// <param name="trackService">Service for adding tracks to playlists.</param>
    /// <param name="playlistHelper">Helper service for managing artist playlists.</param>
    /// <param name="playlistService">Service for clearing playlists.</param>
    /// <param name="logger">Logger instance for recording operations.</param>
    public ArtistTrackOrganizationService(
        ITrackCategorizationService categorizationService,
        ITrackService trackService,
        IPlaylistHelper playlistHelper,
        IPlaylistService playlistService,
        ILogger<ArtistTrackOrganizationService> logger
    )
    {
        _categorizationService = categorizationService;
        _trackService = trackService;
        _playlistHelper = playlistHelper;
        _playlistService = playlistService;
        _logger = logger;
    }

    /// <summary>
    /// Organizes tracks from a specific artist into three playlists based on popularity.
    /// </summary>
    /// <param name="allTracks">All user tracks to filter.</param>
    /// <param name="artistId">The unique identifier of the artist.</param>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>
    /// <c>true</c> if all tracks were successfully organized; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method:
    /// 1. Gets or creates three playlists for the artist (less, medium, more)
    /// 2. Clears existing tracks from the playlists
    /// 3. Converts infrastructure models to domain entities
    /// 4. Uses domain service to categorize artist tracks
    /// 5. Adds categorized tracks to respective playlists
    /// </remarks>
    public async Task<bool> OrganizeArtistTracksAsync(
        IList<SavedTrack> allTracks,
        string artistId,
        SpotifyClient spotifyClient
    )
    {
        _logger.LogInformation("Organizing tracks for artist: {ArtistId}", artistId);

        // Get or create artist playlists
        var artistPlaylists = await _playlistHelper.GetOrCreateArtistPlaylistsAsync(
            spotifyClient,
            artistId
        );

        // Clear existing tracks from playlists
        foreach (var (category, playlistId) in artistPlaylists)
        {
            _logger.LogInformation("Clearing playlist {PlaylistId} for category {Category}", playlistId, category);
            var cleared = await _playlistService.RemoveAllTracksAsync(playlistId);

            if (cleared != models.RemoveAllTracksResponse.Success)
            {
                _logger.LogWarning("Failed to clear playlist {PlaylistId} for category {Category}", playlistId, category);
                return false;
            }
        }

        // Convert to domain entities
        var domainTracks = SpotifyTrackMapper.ToDomain(allTracks);

        // Categorize using domain service (business logic)
        var categorizedTracks = _categorizationService.CategorizeArtistTracks(
            domainTracks,
            artistId
        );

        // Add tracks to respective playlists
        var results = new List<bool>();

        foreach (var (category, playlistId) in artistPlaylists)
        {
            if (!categorizedTracks.TryGetValue(category, out var tracks) || !tracks.Any())
            {
                _logger.LogInformation("No tracks found for category {Category}", category);
                results.Add(true); // No tracks to add is not an error
                continue;
            }

            // Convert back to infrastructure models
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

