using SpotifyAPI.Web;

namespace tracksByPopularity;

public abstract class Constants
{
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
            Scopes.UserTopRead,
            Scopes.PlaylistModifyPrivate,
            Scopes.PlaylistModifyPublic,
        ];

    public static string PlaylistIdLess { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!;

    public static string PlaylistIdLessMedium { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS_MEDIUM")!;

    public static string PlaylistIdMoreMedium { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE_MEDIUM")!;

    public static string PlaylistIdMore { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!;

    public static string PlaylistIdTopShort { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_TOP_SHORT")!;

    public static string PlaylistIdTopMedium { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_TOP_MEDIUM")!;

    public static string PlaylistIdTopLong { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_TOP_LONG")!;

    public static string RedisConnectionString { get; } =
        Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")!;

    public static string MicroserviceClearSongsBaseUrl { get; } =
        Environment.GetEnvironmentVariable("MICROSERVICE_CLEAR_SONGS_BASE_URL")!;

    public static string MicroserviceAuthSpotifyBaseUrl { get; } =
        Environment.GetEnvironmentVariable("MICROSERVICE_AUTH_SPOTIFY_BASE_URL")!;
}
