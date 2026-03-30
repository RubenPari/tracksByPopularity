using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Presentation.Filters;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for playlist backup operations.
/// Uses ISP: Injects only IPlaylistCacheService needed for cache invalidation.
/// </summary>
[ApiController]
[Route("api/backup")]
public class BackupController(
    IPlaylistBackupService backupService,
    IPlaylistCacheService playlistCacheService) : ControllerBase
{
    [HttpGet("list")]
    [SpotifyAuth]
    [ResponseCache(Duration = 60)] // Cache snapshots list for 1 minute
    public async Task<ActionResult<ApiResponse<IEnumerable<Application.DTOs.PlaylistSnapshot>>>> GetSnapshots()
    {
        var spotifyUserId = HttpContext.GetSpotifyUserId();
        var snapshots = await backupService.GetSnapshotsAsync(spotifyUserId);

        var summaries = snapshots.Select(s => new Application.DTOs.PlaylistSnapshot
        {
            Id = s.Id,
            PlaylistId = s.PlaylistId,
            PlaylistName = s.PlaylistName,
            OperationType = s.OperationType,
            CreatedAt = s.CreatedAt,
            TrackCount = s.TrackCount,
            TrackUris = []
        }).ToList();

        return Ok(ApiResponse<IEnumerable<Application.DTOs.PlaylistSnapshot>>.Ok(summaries));
    }

    [HttpPost("restore/{snapshotId}")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse>> RestoreSnapshot(string snapshotId)
    {
        var spotifyUserId = HttpContext.GetSpotifyUserId();
        var spotifyClient = HttpContext.GetSpotifyClient();
        var restored = await backupService.RestoreSnapshotAsync(snapshotId, spotifyUserId, spotifyClient);

        if (restored)
        {
            // Invalidate playlists cache after restore (ISP: only inject what we need)
            await playlistCacheService.InvalidateAsync(spotifyUserId);
            return Ok(ApiResponse.Ok("Playlist restored successfully"));
        }

        return BadRequest(ApiResponse.Fail("Snapshot not found or restore failed"));
    }

    [HttpDelete("{snapshotId}")]
    [SpotifyAuth]
    public async Task<ActionResult<ApiResponse>> DeleteSnapshot(string snapshotId)
    {
        var spotifyUserId = HttpContext.GetSpotifyUserId();
        var deleted = await backupService.DeleteSnapshotAsync(snapshotId, spotifyUserId);

        if (deleted)
        {
            return Ok(ApiResponse.Ok("Snapshot deleted successfully"));
        }

        return BadRequest(ApiResponse.Fail("Snapshot not found or delete failed"));
    }
}
