using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Application.Interfaces;

public interface IPlaylistBackupService
{
    Task<string> CreateSnapshotAsync(string playlistId, SpotifyClient spotifyClient, string operationType);
    Task<IList<PlaylistSnapshot>> GetSnapshotsAsync(string userId);
    Task<bool> RestoreSnapshotAsync(string snapshotId, string userId, SpotifyClient spotifyClient);
}
