using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public static class TrackService
{
    public static async Task<IList<SavedTrack>> GetAllUserTracks(SpotifyClient spotifyClient)
    {
        var firstPageTracks = await spotifyClient.Library.GetTracks();
        return await spotifyClient.PaginateAll(firstPageTracks);
    }

    public static async Task<bool> AddTracksToPlaylist(
        string playlistId,
        IList<SavedTrack> tracks,
        SpotifyClient spotifyClient
    )
    {
        for (var i = 0; i < tracks.Count; i += 100)
        {
            var tracksToAdd = tracks.Skip(i).Take(100).Select(track => track.Track.Uri).ToList();

            if (tracksToAdd.Count <= 0)
                continue;

            var added = await spotifyClient.Playlists.AddItems(
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
