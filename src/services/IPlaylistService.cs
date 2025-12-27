using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public interface IPlaylistService
{
    Task<RemoveAllTracksResponse> RemoveAllTracksAsync(string playlistId);
    Task<bool> CreatePlaylistTracksMinorAsync(
        SpotifyClient spotifyClient,
        IList<SavedTrack> tracks
    );
}

