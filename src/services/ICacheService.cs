using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public interface ICacheService
{
    Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient
    );
}

