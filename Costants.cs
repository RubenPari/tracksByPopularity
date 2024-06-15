using SpotifyAPI.Web;

namespace tracksByPopularity
{
    using System.Collections.Generic;

    public abstract class Costants
    {
        public static SpotifyClientConfig Config { get; } = SpotifyClientConfig.CreateDefault();
        public static string TitleApi => "TracksByPopularityAPI";
        public static int TracksLessPopularity => 25;
        public static int TracksLessMediumPopularity => 50;
        public static int TracksMoreMediumPopularity => 75;

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

        public static string ClientSecret { get; } =
            Environment.GetEnvironmentVariable("CLIENT_SECRET")!;

        public static string RedirectUri { get; } =
            Environment.GetEnvironmentVariable("REDIRECT_URI")!;

        public static string PlaylistIdLess { get; } =
            Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!;

        public static string PlaylistIdLessMedium { get; } =
            Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS_MEDIUM")!;

        public static string PlaylistIdMoreMedium { get; } =
            Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE_MEDIUM")!;

        public static string PlaylistIdMore { get; } =
            Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!;
    }
}
