using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using tracksByPopularity.Presentation.Filters;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for track-related operations.
/// Handles requests to organize tracks into playlists based on popularity levels.
/// </summary>
[ApiController]
[Route("api/track")]
public class TrackController(
    ICacheService cacheService,
    ITrackOrganizationService trackOrganizationService,
    IArtistTrackOrganizationService artistTrackOrganizationService,
    ILogger<TrackController> logger)
    : ControllerBase
{
    /// <summary>
    /// Adds tracks to the designated playlist based on the specified popularity range.
    /// Available ranges: "less" (0-20), "less-medium" (21-40), "medium" (41-60), "more-medium" (41-80), "more" (81-100).
    /// </summary>
    [HttpPost("popularity/{range}")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse>> AddTracksByPopularity(string range)
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
            logger.LogWarning("Invalid popularity range requested: {Range}", range);
            return BadRequest(ApiResponse.Fail($"Invalid popularity range: {range}. Valid values are: less, less-medium, medium, more-medium, more."));
        }

        var spotifyClient = HttpContext.GetSpotifyClient();
        var spotifyUserId = HttpContext.GetSpotifyUserId();
        var allTracks = await cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

        var added = await trackOrganizationService.OrganizeTracksByPopularityAsync(
            spotifyUserId,
            allTracks,
            popularityRange,
            spotifyClient
        );

        if (added) return Ok(ApiResponse.Ok("Tracks added to playlist"));

        logger.LogWarning("Failed to add tracks to playlist for popularity range: {Range}", range);
        return BadRequest(ApiResponse.Fail("Failed to add tracks to playlist"));
    }

    /// <summary>
    /// Returns a list of unique artists from the user's saved tracks, sorted by track count.
    /// </summary>
    [HttpGet("artists")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse<IEnumerable<ArtistSummary>>>> GetLibraryArtists()
    {
        var spotifyClient = HttpContext.GetSpotifyClient();

        // Fetch all followed artists (cursor-paginated)
        var followedIds = new HashSet<string>();
        string? after = null;
        while (true)
        {
            var followedRequest = new FollowOfCurrentUserRequest { Limit = 50 };
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

        var allTracks = await cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

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
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse>> Artist([FromQuery] AddTracksByArtistRequest request)
    {
        var spotifyClient = HttpContext.GetSpotifyClient();
        var spotifyUserId = HttpContext.GetSpotifyUserId();
        var allTracks = await cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

        var added = await artistTrackOrganizationService.OrganizeArtistTracksAsync(
            spotifyUserId,
            allTracks,
            request.ArtistId,
            spotifyClient
        );

        if (added) return Ok(ApiResponse.Ok("Tracks added to playlist"));

        logger.LogWarning("Failed to organize tracks for artist: {ArtistId}", request.ArtistId);
        return BadRequest(ApiResponse.Fail("Failed to add tracks to playlist"));
    }
}
