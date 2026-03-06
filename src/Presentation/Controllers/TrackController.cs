using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.Services;
using tracksByPopularity.Domain.ValueObjects;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Services;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for track-related operations.
/// Handles requests to organize tracks into playlists based on popularity levels.
/// </summary>
[ApiController]
[Route("api/track")]
[Route("track")] // Legacy route for backward compatibility
public class TrackController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ITrackOrganizationService _trackOrganizationService;
    private readonly IArtistTrackOrganizationService _artistTrackOrganizationService;
    private readonly ILogger<TrackController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackController"/> class.
    /// </summary>
    /// <param name="cacheService">Service for retrieving cached user tracks.</param>
    /// <param name="trackOrganizationService">Application service for organizing tracks by popularity.</param>
    /// <param name="artistTrackOrganizationService">Application service for organizing artist tracks.</param>
    /// <param name="logger">Logger instance for recording controller activities.</param>
    public TrackController(
        ICacheService cacheService,
        ITrackOrganizationService trackOrganizationService,
        IArtistTrackOrganizationService artistTrackOrganizationService,
        ILogger<TrackController> logger
    )
    {
        _cacheService = cacheService;
        _trackOrganizationService = trackOrganizationService;
        _artistTrackOrganizationService = artistTrackOrganizationService;
        _logger = logger;
    }

    /// <summary>
    /// Adds tracks to the designated playlist based on the specified popularity range.
    /// Available ranges: "less" (0-20), "less-medium" (21-40), "medium" (41-60), "more-medium" (41-80), "more" (81-100).
    /// </summary>
    /// <param name="range">The popularity range string identifier.</param>
    /// <param name="request">The request containing the playlist ID to add tracks to.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were added successfully
    /// - 400 Bad Request if the operation failed, validation failed, or the range is invalid
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    [HttpPost("popularity/{range}")]
    public async Task<ActionResult<ApiResponse>> AddTracksByPopularity(string range, [FromBody] AddTracksByPopularityRequest request)
    {
        var popularityRange = range.ToLowerInvariant() switch
        {
            "less" => PopularityRange.Less,
            "less-medium" => PopularityRange.LessMedium,
            "medium" => PopularityRange.Medium,
            "more-medium" => PopularityRange.MoreMedium,
            "more" => PopularityRange.More,
            _ => null
        };

        if (popularityRange == null)
        {
            _logger.LogWarning("Invalid popularity range requested: {Range}", range);
            return BadRequest(ApiResponse.Fail($"Invalid popularity range: {range}. Valid values are: less, less-medium, medium, more-medium, more."));
        }

        var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
        var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

        var added = await _trackOrganizationService.OrganizeTracksByPopularityAsync(
            allTracks,
            popularityRange,
            request.PlaylistId,
            spotifyClient
        );

        if (added) return Ok(ApiResponse.Ok("Tracks added to playlist"));

        _logger.LogWarning("Failed to add tracks to playlist for popularity range: {Range}", range);
        return BadRequest(ApiResponse.Fail("Failed to add tracks to playlist"));
    }

    /// <summary>
    /// Organizes tracks from a specific artist into three playlists based on popularity:
    /// - "less": tracks with popularity ≤ 33
    /// - "medium": tracks with popularity 34-66
    /// - "more": tracks with popularity > 66
    /// </summary>
    /// <param name="request">The request containing the artist ID to process.</param>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were organized successfully
    /// - 400 Bad Request if the artist ID is invalid or operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    /// <remarks>
    /// This endpoint:
    /// 1. Validates the artist ID parameter using FluentValidation
    /// 2. Gets or creates three playlists for the artist (less, medium, more)
    /// 3. Clears existing tracks from the playlists
    /// 4. Filters all user tracks to find tracks by the specified artist
    /// 5. Categorizes tracks by popularity and adds them to respective playlists
    /// </remarks>
    [HttpPost("artist")]
    public async Task<ActionResult<ApiResponse>> Artist([FromQuery] AddTracksByArtistRequest request)
    {
        var artistId = request.ArtistId;

        var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
        var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

        var added = await _artistTrackOrganizationService.OrganizeArtistTracksAsync(
            allTracks,
            artistId,
            spotifyClient
        );

        if (added) return Ok(ApiResponse.Ok("Tracks added to playlist"));

        _logger.LogWarning("Failed to organize tracks for artist: {ArtistId}", artistId);
        return BadRequest(ApiResponse.Fail("Failed to add tracks to playlist"));
    }
}

