using SpotifyAPI.Web;

namespace tracksByPopularity.services.Track;

public interface ITrackService
{
    Task<IList<SavedTrack>> GetAllUserTracks();
    Task<IList<FullTrack>> GetTop50UserTracks(TimeRange? timeRange);
}