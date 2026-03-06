using SpotifyAPI.Web;

namespace tracksByPopularity.Infrastructure.Helpers;

public abstract class TrackUtils
{
    public static IList<string> ConvertTracksToUris(List<SavedTrack> tracks)
    {
        return tracks.Select(track => track.Track.Uri).ToList();
    }
}
