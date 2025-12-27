using tracksByPopularity.models;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for clearing playlists.
/// Orchestrates playlist clearing operations.
/// </summary>
public class PlaylistClearingService : IPlaylistClearingService
{
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<PlaylistClearingService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistClearingService"/> class.
    /// </summary>
    /// <param name="playlistService">The playlist service for removing tracks.</param>
    /// <param name="logger">Logger instance for recording clearing operations.</param>
    public PlaylistClearingService(
        IPlaylistService playlistService,
        ILogger<PlaylistClearingService> logger
    )
    {
        _playlistService = playlistService;
        _logger = logger;
    }

    /// <summary>
    /// Clears a playlist by removing all tracks.
    /// </summary>
    /// <param name="playlistId">The unique identifier of the playlist to clear.</param>
    /// <returns>
    /// A <see cref="RemoveAllTracksResponse"/> indicating the result of the operation.
    /// </returns>
    public async Task<RemoveAllTracksResponse> ClearPlaylistAsync(string playlistId)
    {
        _logger.LogInformation("Clearing playlist with ID: {PlaylistId}", playlistId);
        
        var result = await _playlistService.RemoveAllTracksAsync(playlistId);
        
        if (result == RemoveAllTracksResponse.Success)
        {
            _logger.LogInformation("Successfully cleared playlist: {PlaylistId}", playlistId);
        }
        else
        {
            _logger.LogWarning("Failed to clear playlist {PlaylistId}. Response: {Response}", playlistId, result);
        }
        
        return result;
    }
}

