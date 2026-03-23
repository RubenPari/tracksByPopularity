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
    private readonly IPlaylistService _playlistService;
    private readonly ILogger<PlaylistController> _logger;

    public PlaylistController(
        IPlaylistService playlistService,
        ILogger<PlaylistController> logger
    )
    {
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

}