using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public static class TrackService
{
    public static async Task<IList<SavedTrack>> GetAllUserTracks(string? artistId = null)
    {
        var firstPageTracks = await Client.Spotify.Library.GetTracks();
        var allTracks = await Client.Spotify.PaginateAll(firstPageTracks);

        return artistId == null
            ? allTracks
            : allTracks
                .Where(track => track.Track.Artists[0].Id == artistId)
                .ToList();
    }

    public static async Task<bool> AddTracksToArtistPlaylists(
        string artistPlaylistId,
        IList<SavedTrack> trackWithPopularity)
    {
        var playlist = await Client.Spotify.Playlists.Get(artistPlaylistId);

        if (playlist.Id is null)
        {
            return false;
        }

        return await AddTracksToPlaylist(playlist.Id, trackWithPopularity);
    }

    public static async Task<bool> AddTracksToPlaylist(
        string playlistId,
        IList<SavedTrack> tracks)
    {
        for (var i = 0; i < tracks.Count; i += 100)
        {
            var tracksToAdd = tracks
                .Skip(i)
                .Take(100)
                .Select(track => track.Track.Uri)
                .ToList();

            var added = await Client.Spotify.Playlists.AddItems(
                playlistId,
                new PlaylistAddItemsRequest(tracksToAdd));

            if (added.SnapshotId == string.Empty)
            {
                return false;
            }
        }

        return true;
    }
}