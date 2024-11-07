using SpotifyAPI.Web;
using tracksByPopularity.services.Spotify;

namespace tracksByPopularity.services.Track;

public class TrackService(SpotifyService spotifyService) : ITrackService
{
    private readonly SpotifyService _spotifyService = spotifyService;

    public async Task<IList<SavedTrack>> GetAllUserTracks()
    {
        var firstPageTracks = await _spotifyService.Client.Library.GetTracks();
        return await _spotifyService.Client.PaginateAll(firstPageTracks);
    }

    public async Task<IList<FullTrack>> GetTop50UserTracks(TimeRange? timeRange)
    {
        var timeRangeParam = timeRange switch
        {
            TimeRange.ShortTerm => PersonalizationTopRequest.TimeRange.ShortTerm,
            TimeRange.MediumTerm => PersonalizationTopRequest.TimeRange.MediumTerm,
            TimeRange.LongTerm => PersonalizationTopRequest.TimeRange.LongTerm,
            _ => throw new Exception("Invalid time range")
        };

        var fistPage = await _spotifyService.Client.Personalization.GetTopTracks(
            new PersonalizationTopRequest { TimeRangeParam = timeRangeParam, Limit = 50 }
        );

        return await _spotifyService.Client.PaginateAll(fistPage);
    }
}