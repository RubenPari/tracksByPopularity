using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Infrastructure.Services;

namespace tracksByPopularity.Presentation.Controllers;

[ApiController]
[Route("api/backup")]
public class BackupController : ControllerBase
{
    private readonly IPlaylistBackupService _backupService;
    private readonly SpotifyAuthService _spotifyAuthService;
    private const string UserIdCookieName = "spotify_user_id";

    public BackupController(IPlaylistBackupService backupService, SpotifyAuthService spotifyAuthService)
    {
        _backupService = backupService;
        _spotifyAuthService = spotifyAuthService;
    }

    [HttpGet("list")]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlaylistSnapshot>>>> GetSnapshots()
    {
        var userId = Request.Cookies[UserIdCookieName];
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<IEnumerable<PlaylistSnapshot>>.Fail("Not authenticated."));
        }

        var snapshots = await _backupService.GetSnapshotsAsync(userId);

        // Return without TrackUris to save bandwidth
        var summaries = snapshots.Select(s => new PlaylistSnapshot
        {
            Id = s.Id,
            PlaylistId = s.PlaylistId,
            PlaylistName = s.PlaylistName,
            OperationType = s.OperationType,
            CreatedAt = s.CreatedAt,
            TrackCount = s.TrackCount,
            TrackUris = []
        }).ToList();

        return Ok(ApiResponse<IEnumerable<PlaylistSnapshot>>.Ok(summaries));
    }

    [HttpPost("restore/{snapshotId}")]
    public async Task<ActionResult<ApiResponse>> RestoreSnapshot(string snapshotId)
    {
        var userId = Request.Cookies[UserIdCookieName];
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse.Fail("Not authenticated."));
        }

        var spotifyClient = await _spotifyAuthService.GetSpotifyClientForUserAsync(userId);
        var restored = await _backupService.RestoreSnapshotAsync(snapshotId, userId, spotifyClient);

        if (restored)
        {
            return Ok(ApiResponse.Ok("Playlist restored successfully"));
        }

        return BadRequest(ApiResponse.Fail("Snapshot not found or restore failed"));
    }
}
