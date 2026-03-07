using SpotifyAPI.Web;

namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Application service interface for creating playlists based on audio features (moods/BPM).
/// </summary>
public interface IAudioFeaturesPlaylistService
{
    /// <summary>
    /// Analyzes the user's library and creates mood-based playlists.
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client.</param>
    /// <returns>True if successful, otherwise false.</returns>
    Task<bool> GenerateMoodPlaylistsAsync(SpotifyClient spotifyClient);
}
