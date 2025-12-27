using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

[ApiController]
[Route("playlist")]
public class PlaylistController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly IPlaylistService _playlistService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<PlaylistController> _logger;

    public PlaylistController(
        ICacheService cacheService,
        IPlaylistService playlistService,
        SpotifyAuthService spotifyAuthService,
        ILogger<PlaylistController> logger
    )
    {
        _cacheService = cacheService;
        _playlistService = playlistService;
        _spotifyAuthService = spotifyAuthService;
        _logger = logger;
    }

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

