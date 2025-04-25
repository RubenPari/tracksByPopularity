using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public static class TrackService
{
    public static async Task<IList<SavedTrack>> GetAllUserTracksWithClient(
        SpotifyClient spotifyClient
    )
    {
        var firstPageTracks = await spotifyClient.Library.GetTracks();
        return await spotifyClient.PaginateAll(firstPageTracks);
    }

    public static async Task<bool> AddTracksToPlaylist(
        SpotifyClient spotifyClient,
        string playlistId,
        IList<SavedTrack> tracks
    )
    {
        for (var i = 0; i < tracks.Count; i += 100)
        {
            var tracksToAdd = tracks.Skip(i).Take(100).Select(track => track.Track.Uri).ToList();

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
