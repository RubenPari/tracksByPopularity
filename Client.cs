using SpotifyAPI.Web;

namespace tracksByPopularity;

public static class Client
{
    public static SpotifyClient Spotify { get; set; } = null!;
    public static string AccessToken { get; set; } = null!;
}