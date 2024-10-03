using SpotifyAPI.Web;

namespace tracksByPopularity.utils;

public abstract class TrackUtils
{
    public static IList<string> ConvertTracksToIds(IList<SavedTrack> tracks)
    {
        return tracks.Select(track => track.Track.Id).ToList();
    }
}
