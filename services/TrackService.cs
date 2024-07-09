using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public static class TrackService
{
    public static async Task<IList<SavedTrack>> GetAllUserTracks()
    {
        var firstPageTracks = await Client.Spotify!.Library.GetTracks();
        return await Client.Spotify.PaginateAll(firstPageTracks);
    }

    public static async Task<bool> AddTracksToPlaylist(string playlistId, IList<SavedTrack> tracks)
    {
        for (var i = 0; i < tracks.Count; i += 100)
        {
            var tracksToAdd = tracks.Skip(i).Take(100).Select(track => track.Track.Uri).ToList();

            var added = await Client.Spotify!.Playlists.AddItems(
                playlistId,
                new PlaylistAddItemsRequest(tracksToAdd)
            );

            if (added.SnapshotId == string.Empty)
            {
                return false;
            }
        }

        return true;
    }

    public static async Task<IList<FullTrack>> GetTopTracks(TimeRangeEnum timeRange)
    {
        var timeRangeParam = timeRange switch
        {
            TimeRangeEnum.LongTerm => PersonalizationTopRequest.TimeRange.LongTerm,
            TimeRangeEnum.MediumTerm => PersonalizationTopRequest.TimeRange.MediumTerm,
            TimeRangeEnum.ShortTerm => PersonalizationTopRequest.TimeRange.ShortTerm,
            _ => throw new NotImplementedException()
        };

        var topTracks = await Client.Spotify!.Personalization.GetTopTracks(
            new PersonalizationTopRequest { TimeRangeParam = timeRangeParam, Limit = 50 }
        );

        return topTracks.Items!;
    }
}
