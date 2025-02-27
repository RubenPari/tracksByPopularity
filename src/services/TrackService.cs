using SpotifyAPI.Web;
using tracksByPopularity.utils;

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
}
