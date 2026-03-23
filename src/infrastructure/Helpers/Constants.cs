using SpotifyAPI.Web;

namespace tracksByPopularity.Infrastructure.Helpers;

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

    public static string RedisHost { get; } = Environment.GetEnvironmentVariable("REDIS_HOST")!;

    public static string RedisPort { get; } = Environment.GetEnvironmentVariable("REDIS_PORT")!;

    public static string RedisPassword { get; } =
        Environment.GetEnvironmentVariable("REDIS_PASSWORD")!;

    public static int Offset => 0;

    public static int LimitInsertPlaylistTracks => 100;
}
