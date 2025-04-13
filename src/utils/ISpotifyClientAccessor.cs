using SpotifyAPI.Web;

namespace tracksByPopularity.utils;

public interface ISpotifyClientAccessor
{
    SpotifyClient GetClient();
}
