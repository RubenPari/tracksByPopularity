using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Presentation.Filters;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for playlist-related operations.
/// Handles requests to create and manage specialized playlists.
/// </summary>
[ApiController]
[Route("api/playlist")]
public class PlaylistController(ICacheService cacheService) : ControllerBase
{
    /// <summary>
    /// Retrieves all playlists owned by the current user.
    /// Results are cached for improved performance.
    /// </summary>
    [HttpGet("all")]
    [SpotifyAuth]
    [ResponseCache(Duration = 15 * 60, VaryByQueryKeys = new[] { "refresh" })]
    public async Task<ActionResult<ApiResponse<IList<PlaylistInfo>>>> GetAllPlaylists()
    {
        var spotifyClient = HttpContext.GetSpotifyClient();
        var spotifyUserId = HttpContext.GetSpotifyUserId();

        var playlists = await cacheService.GetUserPlaylistsWithCacheAsync(spotifyClient, spotifyUserId);

        // Set cache headers for client-side caching
        Response.Headers["Cache-Control"] = "public, max-age=300";
        Response.Headers["ETag"] = $"\"playlists-{playlists.Count}-{playlists.Sum(p => p.TotalTracks)}\"";

        return Ok(ApiResponse<IList<PlaylistInfo>>.Ok(playlists));
    }

    /// <summary>
    /// Forces a refresh of the playlists cache.
    /// </summary>
    [HttpPost("refresh")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse<IList<PlaylistInfo>>>> RefreshPlaylists()
    {
        var spotifyClient = HttpContext.GetSpotifyClient();
        var spotifyUserId = HttpContext.GetSpotifyUserId();

        // Invalidate and refetch
        await cacheService.InvalidatePlaylistsCacheAsync(spotifyUserId);
        var playlists = await cacheService.GetUserPlaylistsWithCacheAsync(spotifyClient, spotifyUserId);

        return Ok(ApiResponse<IList<PlaylistInfo>>.Ok(playlists));
    }
}
