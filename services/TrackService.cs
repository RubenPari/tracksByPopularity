using System.Net.Http.Headers;
using Newtonsoft.Json;
using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public static class TrackService
{
    private const string PlaylistUrlTemplate = "https://api.spotify.com/v1/playlists/{0}/tracks";

    public static async Task<IList<SavedTrack>> GetAllUserTracks()
    {
        var firstPageTracks = await Client.Spotify.Library.GetTracks();

        return await Client.Spotify.PaginateAll(firstPageTracks);
    }

    public static async Task<bool> AddTracksToPlaylist(IList<SavedTrack> tracks, string playlistId)
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

    private static async Task<List<string>> GetAllTracksFromPlaylist(string playlistId)
    {
        var accessToken = Client.AccessToken;
        var httpClient = new HttpClient();

        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var allTracksUriPlaylist = new List<string>();
        var nextUrl = string.Format(PlaylistUrlTemplate, playlistId);

        while (!string.IsNullOrEmpty(nextUrl))
        {
            var response = await httpClient.GetAsync(nextUrl);
            response.EnsureSuccessStatusCode();

            var responseContent = await response.Content.ReadAsStringAsync();
            var playlistResponse = JsonConvert.DeserializeObject<PlaylistResponse>(responseContent);

            allTracksUriPlaylist.AddRange(playlistResponse!.Items.Select(item => item.Track.Uri));

            nextUrl = playlistResponse.Next;
        }

        return allTracksUriPlaylist;
    }

    public static async Task<bool> RemoveTracksFromPlaylist(string playlistId)
    {
        var allTracks = await GetAllTracksFromPlaylist(playlistId);

        for (var i = 0; i < allTracks.Count; i += 100)
        {
            IList<PlaylistRemoveItemsRequest.Item> tracksToRemove = allTracks
                .Skip(i)
                .Take(100)
                .Select(track => new PlaylistRemoveItemsRequest.Item { Uri = track })
                .ToList();

            var removeItemsRequest = new PlaylistRemoveItemsRequest { Tracks = tracksToRemove };

            var removed = await Client.Spotify.Playlists.RemoveItems(
                playlistId,
                removeItemsRequest);

            if (removed.SnapshotId == string.Empty)
            {
                return false;
            }
        }

        return true;
    }

    public static async Task<bool> RemoveUserTracks(List<SavedTrack> trackWithPopularity)
    {
        var tracksToRemove = trackWithPopularity.Select(track => track.Track.Uri).ToList();

        for (var i = 0; i < tracksToRemove.Count; i += 100)
        {
            var tracks = tracksToRemove
                .Skip(i)
                .Take(100)
                .ToList();

            var removed = await Client.Spotify.Library.RemoveTracks(
                new LibraryRemoveTracksRequest(tracks));

            if (removed == false)
            {
                return false;
            }
        }

        return true;
    }
}