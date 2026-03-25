using SpotifyAPI.Web;

namespace tracksByPopularity.Presentation.Filters;

public static class HttpContextSpotifyExtensions
{
    extension(HttpContext context)
    {
        /// <summary>
        /// Gets the Spotify client from the context.
        /// </summary>
        public SpotifyClient GetSpotifyClient()
            => (SpotifyClient)context.Items[SpotifyAuthFilter.SpotifyClientKey]!;

        /// <summary>
        /// Gets the Spotify user id from the context.
        /// </summary>
        public string GetSpotifyUserId()
            => (string)context.Items[SpotifyAuthFilter.SpotifyUserIdKey]!;
    }
}
