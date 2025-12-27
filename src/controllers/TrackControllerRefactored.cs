using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.models;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

[ApiController]
[Route("track")]
public class TrackController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ITrackService _trackService;
    private readonly IPlaylistHelper _playlistHelper;
    private readonly IPlaylistService _playlistService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<TrackController> _logger;

    public TrackController(
        ICacheService cacheService,
        ITrackService trackService,
        IPlaylistHelper playlistHelper,
        IPlaylistService playlistService,
        SpotifyAuthService spotifyAuthService,
        ILogger<TrackController> logger
    )
    {
        _cacheService = cacheService;
        _trackService = trackService;
        _playlistHelper = playlistHelper;
        _playlistService = playlistService;
        _spotifyAuthService = spotifyAuthService;
        _logger = logger;
    }

    [HttpPost("less")]
    public async Task<IActionResult> Less()
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            var allTracks = await _cacheService.GetAllUserTracksWithClientAsync(spotifyClient);

            var trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity <= Constants.TracksLessPopularity)
                .ToList();

            var added = await _trackService.AddTracksToPlaylistAsync(
                spotifyClient,
                Constants.PlaylistIdLess,
                trackWithPopularity
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

    [HttpPost("artist")]
    public async Task<IActionResult> Artist([FromQuery] string artistId)
    {
        if (string.IsNullOrWhiteSpace(artistId))
        {
            return BadRequest(new { success = false, error = "Artist ID is required" });
        }

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

