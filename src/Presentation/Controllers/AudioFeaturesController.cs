using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Application.Services;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for audio features operations.
/// Exposes endpoints to generate playlists based on tracks' mood and energy.
/// </summary>
[ApiController]
[Route("api/audio-features")]
public class AudioFeaturesController : ControllerBase
{
    private readonly IAudioFeaturesPlaylistService _audioFeaturesService;
    private readonly ILogger<AudioFeaturesController> _logger;

    public AudioFeaturesController(
        IAudioFeaturesPlaylistService audioFeaturesService,
        ILogger<AudioFeaturesController> logger)
    {
        _audioFeaturesService = audioFeaturesService;
        _logger = logger;
    }

    /// <summary>
    /// Analyzes the user's library and generates mood-based playlists (e.g. High Energy, Chill).
    /// </summary>
    /// <returns>A standard API response indicating success or failure.</returns>
    [HttpPost("generate-mood-playlists")]
    public async Task<ActionResult<ApiResponse>> GenerateMoodPlaylists()
    {
        var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
        
        var success = await _audioFeaturesService.GenerateMoodPlaylistsAsync(spotifyClient);

        if (success)
        {
            return Ok(ApiResponse.Ok("Successfully generated mood playlists."));
        }

        return BadRequest(ApiResponse.Fail("Failed to generate mood playlists. Check logs for details."));
    }
}
