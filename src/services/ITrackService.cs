using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public interface ITrackService
{
    Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(SpotifyClient spotifyClient);
    Task<bool> AddTracksToPlaylistAsync(
        SpotifyClient spotifyClient,
        string playlistId,
        IList<SavedTrack> tracks
    );
}

