using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.Services;
using tracksByPopularity.Application.Services;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for playlist-related operations.
/// Handles requests to create and manage specialized playlists.
/// </summary>
[ApiController]
[Route("api/playlist")]
[Route("playlist")] // Legacy route for backward compatibility
public class PlaylistController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IMinorSongsPlaylistService _minorSongsPlaylistService;
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<PlaylistController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="PlaylistController"/> class.
    /// </summary>
    /// <param name="cacheService">Service for retrieving cached user tracks.</param>
    /// <param name="minorSongsPlaylistService">Application service for creating MinorSongs playlist.</param>
    /// <param name="playlistService">Service for playlist management operations.</param>
    /// <param name="logger">Logger instance for recording controller activities.</param>
    public PlaylistController(
        ICacheService cacheService,
        IMinorSongsPlaylistService minorSongsPlaylistService,
        IPlaylistService playlistService,
        ILogger<PlaylistController> logger
    )
    {
        _cacheService = cacheService;
        _minorSongsPlaylistService = minorSongsPlaylistService;
        _playlistService = playlistService;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all playlists owned by the current user.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with a list of user playlists
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IList<PlaylistInfo>>>> GetAllPlaylists()
    {
        var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
        var playlists = await _playlistService.GetAllUserPlaylistsAsync(spotifyClient);

        return Ok(ApiResponse<IList<PlaylistInfo>>.Ok(playlists));
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
    public async Task<ActionResult<ApiResponse>> CreatePlaylistTrackMinor()
    {
        var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
        var tracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

        var created = await _minorSongsPlaylistService.CreateOrUpdateMinorSongsPlaylistAsync(
            tracks,
            spotifyClient
        );

        if (created) return Ok(ApiResponse.Ok("Tracks added to playlist"));

        _logger.LogWarning("Failed to create playlist with minor tracks");
        return BadRequest(ApiResponse.Fail("Failed to add tracks to playlist"));
    }
}