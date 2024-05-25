using SpotifyAPI.Web;

namespace tracksByPopularity
{
    using System.Collections.Generic;

    public class Costants
    {
        public static SpotifyClientConfig Config { get; } = SpotifyClientConfig.CreateDefault();
        public static string TitleApi { get; } = "TracksByPopularityAPI";
        public static int TracksLessPopularity { get; } = 33;
        public static int TracksMediumPopularity { get; } = 66;
        public static List<string> MyScopes { get; } =
        [
            Scopes.UserReadEmail,
            Scopes.UserReadPrivate,
            Scopes.UserLibraryRead,
            Scopes.UserLibraryModify,
            Scopes.PlaylistModifyPrivate,
            Scopes.PlaylistModifyPublic
        ];
        public static string ClientId { get; } = Environment.GetEnvironmentVariable("CLIENT_ID")!;
        public static string ClientSecret { get; } = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;
        public static string RedirectUri { get; } = Environment.GetEnvironmentVariable("REDIRECT_URI")!;
        public static string PlaylistIdLess { get; } = Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!;
        public static string PlaylistIdMedium { get; } = Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM")!;
        public static string PlaylistIdMore { get; } = Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!;
    }
}
