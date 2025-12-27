using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.domain.services;
using tracksByPopularity.domain.valueobjects;
using tracksByPopularity.infrastructure.mappers;
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
public class TrackControllerV2 : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ITrackService _trackService;
    private readonly IPlaylistHelper _playlistHelper;
    private readonly IPlaylistService _playlistService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ITrackCategorizationService _categorizationService;
    private readonly ILogger<TrackControllerV2> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackControllerV2"/> class.
    /// </summary>
    /// <param name="cacheService">Service for retrieving cached user tracks.</param>
    /// <param name="trackService">Service for track-related operations.</param>
    /// <param name="playlistHelper">Helper service for managing artist playlists.</param>
    /// <param name="playlistService">Service for playlist management operations.</param>
    /// <param name="spotifyAuthService">Service for Spotify authentication.</param>
    /// <param name="categorizationService">Domain service for categorizing tracks by popularity.</param>
    /// <param name="logger">Logger instance for recording controller activities.</param>
    public TrackControllerV2(
        ICacheService cacheService,
        ITrackService trackService,
        IPlaylistHelper playlistHelper,
        IPlaylistService playlistService,
        SpotifyAuthService spotifyAuthService,
        ITrackCategorizationService categorizationService,
        ILogger<TrackControllerV2> logger
    )
    {
        _cacheService = cacheService;
        _trackService = trackService;
        _playlistHelper = playlistHelper;
        _playlistService = playlistService;
        _spotifyAuthService = spotifyAuthService;
        _categorizationService = categorizationService;
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

            // Get tracks from cache (infrastructure concern)
            var allSavedTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            // Convert to domain entities (infrastructure -> domain)
            var domainTracks = SpotifyTrackMapper.ToDomain(allSavedTracks);

            // Categorize using domain service (business logic)
            var categorizedTracks = _categorizationService.CategorizeByPopularity(
                domainTracks,
                PopularityRange.Less
            ).ToList();

            // Convert back to infrastructure models for API call
            var tracksToAdd = allSavedTracks.Where(savedTrack =>
                categorizedTracks.Any(dt => dt.Id == savedTrack.Track.Id)
            ).ToList();

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                Constants.PlaylistIdLess,
                tracksToAdd
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

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessPopularity
                    && track.Track.Popularity <= Constants.TracksLessMediumPopularity
                )
                .ToList();

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                Constants.PlaylistIdLessMedium,
                trackWithPopularity
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

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessMediumPopularity
                    && track.Track.Popularity <= Constants.TracksMediumPopularity
                )
                .ToList();

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                Constants.PlaylistIdMedium,
                trackWithPopularity
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

            var trackWithPopularity = allTracks
                .Where(track =>
                    track.Track.Popularity > Constants.TracksLessMediumPopularity
                    && track.Track.Popularity <= Constants.TracksMoreMediumPopularity
                )
                .ToList();

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                Constants.PlaylistIdMoreMedium,
                trackWithPopularity
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

            var trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity > Constants.TracksMoreMediumPopularity)
                .ToList();

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                Constants.PlaylistIdMore,
                trackWithPopularity
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

            var idsArtistPlaylists = await _playlistHelper.GetOrCreateArtistPlaylistsAsync(
                spotifyClient,
                artistId
            );

            // Clear artist playlists if they don't empty
            foreach (var (_, id) in idsArtistPlaylists)
            {
                var cleared = await _playlistService.RemoveAllTracksAsync(id);

                if (cleared != RemoveAllTracksResponse.Success)
                {
                    _logger.LogWarning("Failed to clear artist playlist before adding new tracks");
                    return BadRequest(new { success = false, error = "Failed to clear artist playlist before added new tracks" });
                }
            }

            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var allArtistTracks = allTracks
                .Where(track => track.Track.Artists.Any(artist => artist.Id == artistId))
                .ToList();

            var trackWithLessPopularity = allArtistTracks
                .Where(track => track.Track.Popularity <= Constants.ArtistTracksLessPopularity)
                .ToList();

            var trackWithMediumPopularity = allArtistTracks
                .Where(track =>
                    track.Track.Popularity > Constants.ArtistTracksLessPopularity
                    && track.Track.Popularity <= Constants.ArtistTracksMediumPopularity
                )
                .ToList();

            var trackWithMorePopularity = allArtistTracks
                .Where(track => track.Track.Popularity > Constants.ArtistTracksMediumPopularity)
                .ToList();

            var addedLess = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                idsArtistPlaylists["less"],
                trackWithLessPopularity
            );

            var addedMedium = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                idsArtistPlaylists["medium"],
                trackWithMediumPopularity
            );

            var addedMore = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                idsArtistPlaylists["more"],
                trackWithMorePopularity
            );

            if (!addedLess || !addedMedium || !addedMore)
            {
                _logger.LogWarning("Failed to add tracks to artist playlists");
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

