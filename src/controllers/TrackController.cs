using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.application.services;
using tracksByPopularity.domain.valueobjects;
using tracksByPopularity.models;
using tracksByPopularity.models.requests;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

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
    private readonly IPlaylistHelper _playlistHelper;
    private readonly IPlaylistService _playlistService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<TrackController> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackController"/> class.
    /// </summary>
    /// <param name="cacheService">Service for retrieving cached user tracks.</param>
    /// <param name="trackOrganizationService">Application service for organizing tracks by popularity.</param>
    /// <param name="artistTrackOrganizationService">Application service for organizing artist tracks.</param>
    /// <param name="playlistHelper">Helper service for managing artist playlists.</param>
    /// <param name="playlistService">Service for playlist management operations.</param>
    /// <param name="spotifyAuthService">Service for Spotify authentication.</param>
    /// <param name="logger">Logger instance for recording controller activities.</param>
    public TrackController(
        ICacheService cacheService,
        ITrackOrganizationService trackOrganizationService,
        IArtistTrackOrganizationService artistTrackOrganizationService,
        IPlaylistHelper playlistHelper,
        IPlaylistService playlistService,
        SpotifyAuthService spotifyAuthService,
        ILogger<TrackController> logger
    )
    {
        _cacheService = cacheService;
        _trackOrganizationService = trackOrganizationService;
        _artistTrackOrganizationService = artistTrackOrganizationService;
        _playlistHelper = playlistHelper;
        _playlistService = playlistService;
        _spotifyAuthService = spotifyAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Adds tracks with low popularity (≤20) to the designated playlist.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were added successfully
    /// - 400 Bad Request if the operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    /// <remarks>
    /// This endpoint:
    /// 1. Retrieves all user tracks (from cache if available)
    /// 2. Filters tracks with popularity ≤ 20
    /// 3. Adds filtered tracks to the "less" popularity playlist
    /// </remarks>
    [HttpPost("less")]
    public async Task<IActionResult> Less()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var added = await _trackOrganizationService.OrganizeTracksByPopularityAsync(
                allTracks,
                PopularityRange.Less,
                Constants.PlaylistIdLess,
                spotifyClient
            );

            if (!added)
            {
                _logger.LogWarning("Failed to add tracks to playlist for less popularity");
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

    /// <summary>
    /// Adds tracks with low-medium popularity (21-40) to the designated playlist.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were added successfully
    /// - 400 Bad Request if the operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    [HttpPost("less-medium")]
    public async Task<IActionResult> LessMedium()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var added = await _trackOrganizationService.OrganizeTracksByPopularityAsync(
                allTracks,
                PopularityRange.LessMedium,
                Constants.PlaylistIdLessMedium,
                spotifyClient
            );

            if (!added)
            {
                _logger.LogWarning("Failed to add tracks to playlist for less-medium popularity");
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

    /// <summary>
    /// Adds tracks with medium popularity (41-60) to the designated playlist.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were added successfully
    /// - 400 Bad Request if the operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    [HttpPost("medium")]
    public async Task<IActionResult> Medium()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var added = await _trackOrganizationService.OrganizeTracksByPopularityAsync(
                allTracks,
                PopularityRange.Medium,
                Constants.PlaylistIdMedium,
                spotifyClient
            );

            if (!added)
            {
                _logger.LogWarning("Failed to add tracks to playlist for medium popularity");
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

    /// <summary>
    /// Adds tracks with medium-high popularity (41-80) to the designated playlist.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were added successfully
    /// - 400 Bad Request if the operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    [HttpPost("more-medium")]
    public async Task<IActionResult> MoreMedium()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var added = await _trackOrganizationService.OrganizeTracksByPopularityAsync(
                allTracks,
                PopularityRange.MoreMedium,
                Constants.PlaylistIdMoreMedium,
                spotifyClient
            );

            if (!added)
            {
                _logger.LogWarning("Failed to add tracks to playlist for more-medium popularity");
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

    /// <summary>
    /// Adds tracks with high popularity (>80) to the designated playlist.
    /// </summary>
    /// <returns>
    /// An <see cref="IActionResult"/> containing:
    /// - 200 OK with success message if tracks were added successfully
    /// - 400 Bad Request if the operation failed
    /// - 401 Unauthorized if authentication failed
    /// </returns>
    [HttpPost("more")]
    public async Task<IActionResult> More()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var added = await _trackOrganizationService.OrganizeTracksByPopularityAsync(
                allTracks,
                PopularityRange.More,
                Constants.PlaylistIdMore,
                spotifyClient
            );

            if (!added)
            {
                _logger.LogWarning("Failed to add tracks to playlist for more popularity");
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
    public async Task<IActionResult> Artist([FromQuery] AddTracksByArtistRequest request)
    {
        // FluentValidation automatically validates and adds errors to ModelState
        // If validation fails, ASP.NET Core will return 400 BadRequest automatically
        // due to [ApiController] attribute, but we can still check for explicit handling
        if (!ModelState.IsValid)
        {
            var errors = ModelState
                .Where(x => x.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value?.Errors.Select(e => e.ErrorMessage).ToArray() ?? Array.Empty<string>()
                );

            return BadRequest(new { success = false, error = "Validation failed", errors });
        }

        var artistId = request.ArtistId;

        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();
            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var added = await _artistTrackOrganizationService.OrganizeArtistTracksAsync(
                allTracks,
                artistId,
                spotifyClient
            );

            if (!added)
            {
                _logger.LogWarning("Failed to organize tracks for artist: {ArtistId}", artistId);
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

