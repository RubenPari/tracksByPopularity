using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Service for playlist backup operations.
/// </summary>
public interface IPlaylistBackupService
{
    /// <summary>
    /// Creates a snapshot of the playlist.
    /// </summary>
    /// <param name="playlistId">The playlist id.</param>
    /// <param name="spotifyClient">The spotify client.</param>
    /// <param name="operationType">The operation type.</param>
    /// <returns></returns>
    Task<string> CreateSnapshotAsync(string playlistId, SpotifyClient spotifyClient, string operationType);
    
    /// <summary>
    /// Gets the snapshots for the user.
    /// </summary>
    /// <param name="userId">The user id.</param>
    /// <returns></returns>
    Task<IList<PlaylistSnapshot>> GetSnapshotsAsync(string userId);
    
    /// <summary>
    /// Restores the snapshot.
    /// </summary>
    /// <param name="snapshotId">The snapshot id.</param>
    /// <param name="userId">The user id.</param>
    /// <param name="spotifyClient">The spotify client.</param>
    /// <returns></returns>
    Task<bool> RestoreSnapshotAsync(string snapshotId, string userId, SpotifyClient spotifyClient);
}
