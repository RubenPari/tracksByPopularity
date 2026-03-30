using Microsoft.EntityFrameworkCore;
using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;
using tracksByPopularity.Domain.Entities;
using tracksByPopularity.Infrastructure.Data;

namespace tracksByPopularity.Infrastructure.Services;

public class PlaylistBackupService : IPlaylistBackupService
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger<PlaylistBackupService> _logger;

    public PlaylistBackupService(AppDbContext dbContext, ILogger<PlaylistBackupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> CreateSnapshotAsync(string spotifyUserId, string playlistId, SpotifyClient spotifyClient, string operationType)
    {
        _logger.LogInformation("Creating snapshot for playlist {PlaylistId} for user {SpotifyUserId}", playlistId, spotifyUserId);

        var firstPage = await spotifyClient.Playlists.GetItems(playlistId);
        var allItems = await spotifyClient.PaginateAll(firstPage);
        var trackUris = allItems
            .Where(item => item.Track is FullTrack)
            .Select((item, index) => new SnapshotTrack
            {
                TrackUri = ((FullTrack)item.Track).Uri,
                OrderIndex = index
            })
            .ToList();

        var playlist = await spotifyClient.Playlists.Get(playlistId);
        var playlistName = playlist?.Name ?? playlistId;

        var user = await _dbContext.Users
            .Include(u => u.SpotifyLink)
            .FirstOrDefaultAsync(u => u.SpotifyLink != null && u.SpotifyLink.SpotifyUserId == spotifyUserId);

        var snapshot = new Domain.Entities.PlaylistSnapshot
        {
            Id = Guid.NewGuid(),
            UserId = user?.Id,
            SpotifyUserId = spotifyUserId,
            PlaylistId = playlistId,
            PlaylistName = playlistName,
            OperationType = operationType,
            CreatedAt = DateTime.UtcNow,
            TrackCount = trackUris.Count,
            Tracks = trackUris
        };

        _dbContext.PlaylistSnapshots.Add(snapshot);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Snapshot {SnapshotId} created for playlist {PlaylistId} with {Count} tracks",
            snapshot.Id, playlistId, trackUris.Count);

        return snapshot.Id.ToString();
    }

    public async Task<IList<Application.DTOs.PlaylistSnapshot>> GetSnapshotsAsync(string spotifyUserId)
    {
        var snapshots = await _dbContext.PlaylistSnapshots
            .Where(s => s.SpotifyUserId == spotifyUserId)
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => new Application.DTOs.PlaylistSnapshot
            {
                Id = s.Id.ToString(),
                PlaylistId = s.PlaylistId,
                PlaylistName = s.PlaylistName,
                OperationType = s.OperationType,
                CreatedAt = s.CreatedAt,
                TrackCount = s.TrackCount,
                TrackUris = s.Tracks.OrderBy(t => t.OrderIndex).Select(t => t.TrackUri).ToList()
            })
            .ToListAsync();

        return snapshots;
    }

    public async Task<bool> RestoreSnapshotAsync(string snapshotId, string spotifyUserId, SpotifyClient spotifyClient)
    {
        if (!Guid.TryParse(snapshotId, out var snapshotGuid))
        {
            _logger.LogWarning("Invalid snapshot ID format: {SnapshotId}", snapshotId);
            return false;
        }

        var snapshot = await _dbContext.PlaylistSnapshots
            .Include(s => s.Tracks)
            .FirstOrDefaultAsync(s => s.Id == snapshotGuid && s.SpotifyUserId == spotifyUserId);

        if (snapshot == null)
        {
            _logger.LogWarning("Snapshot {SnapshotId} not found for user {SpotifyUserId}", snapshotId, spotifyUserId);
            return false;
        }

        _logger.LogInformation("Restoring snapshot {SnapshotId} for playlist {PlaylistId}", snapshotId, snapshot.PlaylistId);

        await spotifyClient.Playlists.ReplaceItems(
            snapshot.PlaylistId,
            new PlaylistReplaceItemsRequest(new List<string>())
        );

        var trackUris = snapshot.Tracks.OrderBy(t => t.OrderIndex).Select(t => t.TrackUri).ToList();
        for (var i = 0; i < trackUris.Count; i += 100)
        {
            var batch = trackUris.Skip(i).Take(100).ToList();
            await spotifyClient.Playlists.AddItems(
                snapshot.PlaylistId,
                new PlaylistAddItemsRequest(batch)
            );
        }

        _logger.LogInformation("Restored {Count} tracks to playlist {PlaylistId}", snapshot.TrackCount, snapshot.PlaylistId);
        return true;
    }

    public async Task<bool> DeleteSnapshotAsync(string snapshotId, string spotifyUserId)
    {
        if (!Guid.TryParse(snapshotId, out var snapshotGuid))
        {
            _logger.LogWarning("Invalid snapshot ID format: {SnapshotId}", snapshotId);
            return false;
        }

        var snapshot = await _dbContext.PlaylistSnapshots
            .FirstOrDefaultAsync(s => s.Id == snapshotGuid && s.SpotifyUserId == spotifyUserId);

        if (snapshot == null)
        {
            _logger.LogWarning("Snapshot {SnapshotId} not found for user {SpotifyUserId}", snapshotId, spotifyUserId);
            return false;
        }

        _dbContext.PlaylistSnapshots.Remove(snapshot);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted snapshot {SnapshotId}", snapshotId);
        return true;
    }

    public async Task<int> DeleteOldSnapshotsAsync(int daysOld)
    {
        var cutoffDate = DateTime.UtcNow.AddDays(-daysOld);
        
        var oldSnapshots = await _dbContext.PlaylistSnapshots
            .Where(s => s.CreatedAt < cutoffDate)
            .ToListAsync();

        if (oldSnapshots.Count == 0)
        {
            return 0;
        }

        _dbContext.PlaylistSnapshots.RemoveRange(oldSnapshots);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted {Count} snapshots older than {Days} days", oldSnapshots.Count, daysOld);
        return oldSnapshots.Count;
    }
}
