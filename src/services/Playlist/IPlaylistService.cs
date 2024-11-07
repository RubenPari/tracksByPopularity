using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.services.Playlist;

public interface IPlaylistService
{
    Task<bool> AddTrackToPlaylist(string playlistId, IList<string> tracks);
    Task<bool> CreatePlaylistTracksMinorAsync(IList<SavedTrack> tracks);
    Task<RemoveAllTracksResponse> RemoveAllTracks(string playlistId);
}