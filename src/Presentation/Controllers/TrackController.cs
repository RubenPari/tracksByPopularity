using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using tracksByPopularity.Application.Services;
using tracksByPopularity.Domain.ValueObjects;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Infrastructure.Services;

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
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<TrackController> _logger;
    private const string UserIdCookieName = "spotify_user_id";

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackController"/> class.
    /// </summary>
    public TrackController(
        ICacheService cacheService,
        ITrackOrganizationService trackOrganizationService,
        IArtistTrackOrganizationService artistTrackOrganizationService,
        SpotifyAuthService spotifyAuthService,
        ILogger<TrackController> logger
    )
    {
        _cacheService = cacheService;
        _trackOrganizationService = trackOrganizationService;
        _artistTrackOrganizationService = artistTrackOrganizationService;
        _spotifyAuthService = spotifyAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Adds tracks to the designated playlist based on the specified popularity range.
    /// Available ranges: "less" (0-20), "less-medium" (21-40), "medium" (41-60), "more-medium" (41-80), "more" (81-100).
    /// </summary>
    [HttpPost("popularity/{range}")]
    public async Task<ActionResult<ApiResponse>> AddTracksByPopularity(string range, [FromBody] AddTracksByPopularityRequest request)
    {
        var userId = Request.Cookies[UserIdCookieName];
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("Not authenticated. Please log in with Spotify."));
        }

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

        var spotifyClient = await _spotifyAuthService.GetSpotifyClientForUserAsync(userId);
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
    /// Returns a list of unique artists from the user's saved tracks, sorted by track count.
    /// </summary>
    [HttpGet("artists")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistSummary>>>> GetLibraryArtists()
    {
        var userId = Request.Cookies[UserIdCookieName];
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<IEnumerable<ArtistSummary>>.Fail("Not authenticated. Please log in with Spotify."));
        }

        var spotifyClient = await _spotifyAuthService.GetSpotifyClientForUserAsync(userId);

        // Fetch all followed artists (cursor-paginated)
        var followedIds = new HashSet<string>();
        string? after = null;
        while (true)
        {
            var followedRequest = new FollowOfCurrentUserRequest(FollowOfCurrentUserRequest.Type.Artist) { Limit = 50 };
            if (after != null) followedRequest.After = after;
            var response = await spotifyClient.Follow.OfCurrentUser(followedRequest);
            var page = response.Artists;
            foreach (var artist in page.Items!)
            {
                followedIds.Add(artist.Id);
            }
            if (page.Cursors?.After == null) break;
            after = page.Cursors.After;
        }

        var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

        var artists = allTracks
            .SelectMany(st => st.Track.Artists.Select(a => new { a.Id, a.Name, TrackId = st.Track.Id }))
            .Where(x => followedIds.Contains(x.Id))
            .GroupBy(x => x.Id)
            .Select(g => new ArtistSummary
            {
                Id = g.Key,
                Name = g.First().Name,
                Count = g.Select(x => x.TrackId).Distinct().Count()
            })
            .OrderByDescending(a => a.Count)
            .ToList();

        return Ok(ApiResponse<IEnumerable<ArtistSummary>>.Ok(artists));
    }

    /// <summary>
    /// Organizes tracks from a specific artist into three playlists based on popularity.
    /// </summary>
    [HttpPost("artist")]
    public async Task<ActionResult<ApiResponse>> Artist([FromQuery] AddTracksByArtistRequest request)
    {
        var userId = Request.Cookies[UserIdCookieName];
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("Not authenticated. Please log in with Spotify."));
        }

        var artistId = request.ArtistId;

        var spotifyClient = await _spotifyAuthService.GetSpotifyClientForUserAsync(userId);
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
