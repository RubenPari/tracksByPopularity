using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Service for playlist backup operations.
/// </summary>
public interface IPlaylistBackupService
{
    /// <summary>
    /// Creates a snapshot of the playlist.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user id.</param>
    /// <param name="playlistId">The playlist id.</param>
    /// <param name="spotifyClient">The spotify client.</param>
    /// <param name="operationType">The operation type.</param>
    /// <returns></returns>
    Task<string> CreateSnapshotAsync(string spotifyUserId, string playlistId, SpotifyClient spotifyClient, string operationType);
    
    /// <summary>
    /// Gets the snapshots for the user.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user id.</param>
    /// <returns></returns>
    Task<IList<DTOs.PlaylistSnapshot>> GetSnapshotsAsync(string spotifyUserId);
    
    /// <summary>
    /// Restores the snapshot.
    /// </summary>
    /// <param name="snapshotId">The snapshot id.</param>
    /// <param name="spotifyUserId">The Spotify user id.</param>
    /// <param name="spotifyClient">The spotify client.</param>
    /// <returns></returns>
    Task<bool> RestoreSnapshotAsync(string snapshotId, string spotifyUserId, SpotifyClient spotifyClient);

    /// <summary>
    /// Deletes a snapshot.
    /// </summary>
    /// <param name="snapshotId">The snapshot id.</param>
    /// <param name="spotifyUserId">The Spotify user id.</param>
    /// <returns></returns>
    Task<bool> DeleteSnapshotAsync(string snapshotId, string spotifyUserId);

    /// <summary>
    /// Deletes snapshots older than the specified number of days.
    /// </summary>
    /// <param name="daysOld">Delete snapshots older than this many days.</param>
    /// <returns></returns>
    Task<int> DeleteOldSnapshotsAsync(int daysOld);
}
