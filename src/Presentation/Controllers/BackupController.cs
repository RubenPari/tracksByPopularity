using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Presentation.Filters;

namespace tracksByPopularity.Presentation.Controllers;

[ApiController]
[Route("api/backup")]
public class BackupController : ControllerBase
{
    private readonly IPlaylistBackupService _backupService;

    public BackupController(IPlaylistBackupService backupService)
    {
        _backupService = backupService;
    }

    [HttpGet("list")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse<IEnumerable<PlaylistSnapshot>>>> GetSnapshots()
    {
        var userId = HttpContext.GetSpotifyUserId();
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
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse>> RestoreSnapshot(string snapshotId)
    {
        var userId = HttpContext.GetSpotifyUserId();
        var spotifyClient = HttpContext.GetSpotifyClient();
        var restored = await _backupService.RestoreSnapshotAsync(snapshotId, userId, spotifyClient);

        if (restored)
        {
            return Ok(ApiResponse.Ok("Playlist restored successfully"));
        }

        return BadRequest(ApiResponse.Fail("Snapshot not found or restore failed"));
    }
}
