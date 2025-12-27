using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

/// <summary>
/// API controller for playlist-related operations.
/// Handles requests to create and manage specialized playlists.
/// </summary>
[ApiController]
[Route("api/playlist")]
public class PlaylistControllerV2 : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IPlaylistService _playlistService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<PlaylistControllerV2> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistControllerV2"/> class.
    /// </summary>
    /// <param name="cacheService">Service for retrieving cached user tracks.</param>
    /// <param name="playlistService">Service for playlist management operations.</param>
    /// <param name="spotifyAuthService">Service for Spotify authentication.</param>
    /// <param name="logger">Logger instance for recording controller activities.</param>
    public PlaylistControllerV2(
        ICacheService cacheService,
        IPlaylistService playlistService,
        SpotifyAuthService spotifyAuthService,
        ILogger<PlaylistControllerV2> logger
    )
    {
        _cacheService = cacheService;
        _playlistService = playlistService;
        _spotifyAuthService = spotifyAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Creates or updates a "MinorSongs" playlist containing tracks from artists
    /// that have 5 or fewer songs in the user's library.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if playlist was created/updated successfully
    /// - 400 Bad Request if the operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    /// <remarks>
    /// This endpoint creates a playlist that helps users discover tracks from
    /// lesser-known artists in their library. The playlist is automatically
    /// created if it doesn't exist, and tracks are added in paginated batches.
    /// </remarks>
    [HttpPost("create-playlist-track-minor")]
    public async Task<IActionResult> CreatePlaylistTrackMinor()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            var tracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var created = await _playlistService.CreatePlaylistTracksMinorAsync(
                spotifyClient,
                tracks
            );

            if (!created)
            {
                _logger.LogWarning("Failed to create playlist with minor tracks");
                return BadRequest(new { success = false, error = "Failed to add tracks to playlist" });
            }

            return Ok(new { success = true, message = "Tracks added to playlist" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized access attempt");
            return Unauthorized(new { success = false, error = "Unauthorized" });
        }
    }
}

