using tracksByPopularity.models;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for clearing playlists.
/// Handles the orchestration of playlist clearing operations.
/// </summary>
public interface IPlaylistClearingService
{
    /// <summary>
    /// Clears a playlist by removing all tracks.
    /// </summary>
    /// <param name="playlistId">The unique identifier of the playlist to clear.</param>
    /// <returns>
    /// A <see cref="RemoveAllTracksResponse"/> indicating the result of the operation.
    /// </returns>
    Task<RemoveAllTracksResponse> ClearPlaylistAsync(string playlistId);
}

