using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.Services;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Infrastructure.Services;

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
    private readonly IPlaylistService _playlistService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<PlaylistController> _logger;
    private const string UserIdCookieName = "spotify_user_id";

    public PlaylistController(
        IPlaylistService playlistService,
        SpotifyAuthService spotifyAuthService,
        ILogger<PlaylistController> logger
    )
    {
        _playlistService = playlistService;
        _spotifyAuthService = spotifyAuthService;
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
        var userId = Request.Cookies[UserIdCookieName];
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<IList<PlaylistInfo>>.Fail("Not authenticated. Please log in with Spotify."));
        }

        var spotifyClient = await _spotifyAuthService.GetSpotifyClientForUserAsync(userId);
        var playlists = await _playlistService.GetAllUserPlaylistsAsync(spotifyClient);

        return Ok(ApiResponse<IList<PlaylistInfo>>.Ok(playlists));
    }

}
