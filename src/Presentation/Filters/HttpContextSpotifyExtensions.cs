using SpotifyAPI.Web;

namespace tracksByPopularity.Presentation.Filters;

public static class HttpContextSpotifyExtensions
{
    public static SpotifyClient GetSpotifyClient(this HttpContext context)
        => (SpotifyClient)context.Items[SpotifyAuthFilter.SpotifyClientKey]!;

    public static string GetSpotifyUserId(this HttpContext context)
        => (string)context.Items[SpotifyAuthFilter.SpotifyUserIdKey]!;
}
