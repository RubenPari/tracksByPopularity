using SpotifyAPI.Web;

namespace tracksByPopularity
{
    using System.Collections.Generic;

    public abstract class Costants
    {
        public static SpotifyClientConfig Config { get; } = SpotifyClientConfig.CreateDefault();
        public static string TitleApi => "TracksByPopularityAPI";
        public static int TracksLessArtistPopularity => 25;
        public static int TracksLessMediumArtistPopularity => 50;
        public static int TracksMoreMediumArtistPopularity => 75;
        public static int TracksLessPopularity => 33;
        public static int TracksMediumPopularity => 66;

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

        public static string PlaylistIdMedium { get; } =
            Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM")!;

        public static string PlaylistIdMore { get; } =
            Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!;
    }
}