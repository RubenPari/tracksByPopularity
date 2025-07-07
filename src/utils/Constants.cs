using SpotifyAPI.Web;

namespace tracksByPopularity.utils;

public abstract class Constants
{
    public static SpotifyClientConfig Config { get; } = SpotifyClientConfig.CreateDefault();
    public static string TitleApi => "TracksByPopularityAPI";
    public static int TracksLessPopularity => 20;
    public static int TracksLessMediumPopularity => 40;
    public static int TracksMediumPopularity => 60;
    public static int TracksMoreMediumPopularity => 80;
    public static int ArtistTracksLessPopularity => 33;
    public static int ArtistTracksMediumPopularity => 66;

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

    public static string ClientId { get; } = Environment.GetEnvironmentVariable("CLIENT_ID")!;

    public static string ClientSecret { get; } =
        Environment.GetEnvironmentVariable("CLIENT_SECRET")!;

    public static string RedirectUri { get; } = Environment.GetEnvironmentVariable("REDIRECT_URI")!;

    public static string PlaylistIdLess { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!;

    public static string PlaylistIdLessMedium { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS_MEDIUM")!;

    public static string PlaylistIdMedium { get; } =
        Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM")!;

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

    public static string RedisHost { get; } =
        Environment.GetEnvironmentVariable("REDIS_HOST")!;

    public static string RedisPort { get; } =
        Environment.GetEnvironmentVariable("REDIS_PORT")!;

    public static string RedisPassword { get; } =
        Environment.GetEnvironmentVariable("REDIS_PASSWORD")!;

    public static string PlaylistNameWithMinorSongs => "MinorSongs";

    public static int Offset => 0;

    public static int LimitInsertPlaylistTracks => 100;
}