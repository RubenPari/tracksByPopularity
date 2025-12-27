using tracksByPopularity.services;
using SpotifyAPI.Web;

namespace tracksByPopularity.application.services;

/// <summary>
/// Application service implementation for creating and managing the MinorSongs playlist.
/// Orchestrates the business logic of creating playlists with tracks from lesser-known artists.
/// </summary>
public class MinorSongsPlaylistService : IMinorSongsPlaylistService
{
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<MinorSongsPlaylistService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="MinorSongsPlaylistService"/> class.
    /// </summary>
    /// <param name="playlistService">Service for playlist management operations.</param>
    /// <param name="logger">Logger instance for recording operations.</param>
    public MinorSongsPlaylistService(
        IPlaylistService playlistService,
        ILogger<MinorSongsPlaylistService> logger
    )
    {
        _playlistService = playlistService;
        _logger = logger;
    }

    /// <summary>
    /// Creates or updates a "MinorSongs" playlist containing tracks from artists
    /// that have 5 or fewer songs in the user's library.
    /// </summary>
    /// <param name="allTracks">All user tracks to filter.</param>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>
    /// <c>true</c> if the playlist was created/updated successfully; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method delegates to IPlaylistService.CreatePlaylistTracksMinorAsync which:
    /// 1. Checks if "MinorSongs" playlist exists, creating it if necessary
    /// 2. Retrieves artist summary to identify artists with ≤5 songs
    /// 3. Filters tracks to include only those from qualifying artists
    /// 4. Adds filtered tracks to the playlist in paginated batches
    /// </remarks>
    public async Task<bool> CreateOrUpdateMinorSongsPlaylistAsync(
        IList<SavedTrack> allTracks,
        SpotifyClient spotifyClient
    )
    {
        _logger.LogInformation("Creating or updating MinorSongs playlist");

        var result = await _playlistService.CreatePlaylistTracksMinorAsync(
            spotifyClient,
            allTracks
        );

        if (result)
        {
            _logger.LogInformation("Successfully created/updated MinorSongs playlist");
        }
        else
        {
            _logger.LogWarning("Failed to create/update MinorSongs playlist");
        }

        return result;
    }
}

