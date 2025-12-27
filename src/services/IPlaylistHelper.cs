using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public interface IPlaylistHelper
{
    Task<Dictionary<string, string>> GetOrCreateArtistPlaylistsAsync(
        SpotifyClient spotifyClient,
        string artistId
    );
}

