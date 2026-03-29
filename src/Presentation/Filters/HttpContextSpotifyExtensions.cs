using SpotifyAPI.Web;

namespace tracksByPopularity.Presentation.Filters;

public static class HttpContextSpotifyExtensions
{
    /// <summary>
    /// Gets the Spotify client from the context.
    /// </summary>
    public static SpotifyClient GetSpotifyClient(this HttpContext context)
        => (SpotifyClient)context.Items[SpotifyAuthFilter.SpotifyClientKey]!;

    /// <summary>
    /// Gets the Spotify user id from the context.
    /// </summary>
    public static string GetSpotifyUserId(this HttpContext context)
        => (string)context.Items[SpotifyAuthFilter.SpotifyUserIdKey]!;
}
