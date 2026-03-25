using SpotifyAPI.Web;

namespace tracksByPopularity.Infrastructure.Helpers;

public abstract class Constants
{
    public static string TitleApi => "TracksByPopularityAPI";
    public static int TracksLessPopularity => 20;
    public static int TracksLessMediumPopularity => 40;
    public static int TracksMediumPopularity => 60;
    public static int TracksMoreMediumPopularity => 80;
    public static List<string> MyScopes { get; } =
        [
            Scopes.UserReadEmail,
            Scopes.UserReadPrivate,
            Scopes.UserLibraryRead,
            Scopes.UserLibraryModify,
            Scopes.UserTopRead,
            Scopes.PlaylistModifyPrivate,
            Scopes.PlaylistModifyPublic,
            Scopes.UserFollowRead,
        ];

    public static int Offset => 0;

    public static int LimitInsertPlaylistTracks => 100;
}
