using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Presentation.Filters;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for playlist-related operations.
/// Handles requests to create and manage specialized playlists.
/// </summary>
[ApiController]
[Route("api/playlist")]
public class PlaylistController(IPlaylistService playlistService) : ControllerBase
{
    /// <summary>
    /// Retrieves all playlists owned by the current user.
    /// </summary>
    [HttpGet("all")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse<IList<PlaylistInfo>>>> GetAllPlaylists()
    {
        var spotifyClient = HttpContext.GetSpotifyClient();
        var playlists = await playlistService.GetAllUserPlaylistsAsync(spotifyClient);

        return Ok(ApiResponse<IList<PlaylistInfo>>.Ok(playlists));
    }
}
