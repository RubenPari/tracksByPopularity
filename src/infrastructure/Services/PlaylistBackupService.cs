using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

public class PlaylistBackupService : IPlaylistBackupService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<PlaylistBackupService> _logger;
    private static readonly TimeSpan SnapshotTtl = TimeSpan.FromDays(7);

    public PlaylistBackupService(IConnectionMultiplexer redis, ILogger<PlaylistBackupService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<string> CreateSnapshotAsync(string playlistId, SpotifyClient spotifyClient, string operationType)
    {
        _logger.LogInformation("Creating snapshot for playlist {PlaylistId}", playlistId);

        // Fetch current playlist tracks
        var firstPage = await spotifyClient.Playlists.GetItems(playlistId);
        var allItems = await spotifyClient.PaginateAll(firstPage);
        var trackUris = allItems
            .Where(item => item.Track is FullTrack)
            .Select(item => ((FullTrack)item.Track).Uri)
            .ToList();

        // Get playlist name
        var playlist = await spotifyClient.Playlists.Get(playlistId);
        var playlistName = playlist?.Name ?? playlistId;

        // Get user ID from the Spotify client
        var userId = (await spotifyClient.UserProfile.Current()).Id;

        var snapshot = new PlaylistSnapshot
        {
            Id = Guid.NewGuid().ToString(),
            PlaylistId = playlistId,
            PlaylistName = playlistName,
            OperationType = operationType,
            CreatedAt = DateTime.UtcNow,
            TrackCount = trackUris.Count,
            TrackUris = trackUris
        };

        var db = _redis.GetDatabase();
        var key = $"backup:{userId}:{snapshot.Id}";
        var json = JsonConvert.SerializeObject(snapshot);
        await db.StringSetAsync(key, json, SnapshotTtl);

        _logger.LogInformation("Snapshot {SnapshotId} created for playlist {PlaylistId} with {Count} tracks",
            snapshot.Id, playlistId, trackUris.Count);

        return snapshot.Id;
    }

    public async Task<IList<PlaylistSnapshot>> GetSnapshotsAsync(string userId)
    {
        var db = _redis.GetDatabase();
        var server = _redis.GetServers().First();
        var pattern = $"backup:{userId}:*";
        var keys = server.Keys(pattern: pattern).ToList();

        var snapshots = new List<PlaylistSnapshot>();
        foreach (var key in keys)
        {
            var json = await db.StringGetAsync(key);
            if (json.HasValue)
            {
                var snapshot = JsonConvert.DeserializeObject<PlaylistSnapshot>(json!);
                if (snapshot != null)
                {
                    snapshots.Add(snapshot);
                }
            }
        }

        return snapshots.OrderByDescending(s => s.CreatedAt).ToList();
    }

    public async Task<bool> RestoreSnapshotAsync(string snapshotId, string userId, SpotifyClient spotifyClient)
    {
        var db = _redis.GetDatabase();
        var key = $"backup:{userId}:{snapshotId}";
        var json = await db.StringGetAsync(key);

        if (!json.HasValue)
        {
            _logger.LogWarning("Snapshot {SnapshotId} not found for user {UserId}", snapshotId, userId);
            return false;
        }

        var snapshot = JsonConvert.DeserializeObject<PlaylistSnapshot>(json!);
        if (snapshot == null) return false;

        _logger.LogInformation("Restoring snapshot {SnapshotId} for playlist {PlaylistId}", snapshotId, snapshot.PlaylistId);

        // Clear the playlist
        await spotifyClient.Playlists.ReplaceItems(
            snapshot.PlaylistId,
            new PlaylistReplaceItemsRequest(new List<string>())
        );

        // Add tracks back in batches of 100
        for (var i = 0; i < snapshot.TrackUris.Count; i += 100)
        {
            var batch = snapshot.TrackUris.Skip(i).Take(100).ToList();
            await spotifyClient.Playlists.AddItems(
                snapshot.PlaylistId,
                new PlaylistAddItemsRequest(batch)
            );
        }

        _logger.LogInformation("Restored {Count} tracks to playlist {PlaylistId}", snapshot.TrackCount, snapshot.PlaylistId);
        return true;
    }
}
