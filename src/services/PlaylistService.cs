using System.Net;
using SpotifyAPI.Web;
using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.utils;

namespace tracksByPopularity.services;

public static class PlaylistService
{
    public static async Task<RemoveAllTracksResponse> RemoveAllTracks(string playlistId)
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(200);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"http://localhost:3000/playlist/delete-tracks?id={playlistId}"),
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => RemoveAllTracksResponse.Unauthorized,
            HttpStatusCode.BadRequest => RemoveAllTracksResponse.BadRequest,
            _ => RemoveAllTracksResponse.Success,
        };
    }

    public static async Task<bool> CreatePlaylistTracksMinorAsync(IList<SavedTrack> tracks)
    {
        var userId = await UserHelper.GetUserId();

        // Check if "MinorSongs" playlist already exists
        var playlistsUserFirstPage = await Client.Spotify!.Playlists.GetUsers(userId);
        var playlistsUser = await Client.Spotify.PaginateAll(playlistsUserFirstPage);

        var idPlaylistMinorSongs = playlistsUser
            .FirstOrDefault(playlist => playlist.Name == Constants.PlaylistNameWithMinorSongs)?.Id;

        if (idPlaylistMinorSongs != null)
        {
            return false;
        }
        else
        {
            // Create playlist "MinorSongs"
            var playlistMinorSongs =
                await Client.Spotify.Playlists.Create(userId,
                    new PlaylistCreateRequest(Constants.PlaylistNameWithMinorSongs));

            idPlaylistMinorSongs = playlistMinorSongs.Id;
        }
        // Add tracks to playlist "MinorSongs"

        // Get artists with less than 5 songs in user library
        var artistsSummary = await ArtistHelper.GetArtistsSummary();

        if (artistsSummary == null)
        {
            return false;
        }

        // Find all tracks that belong to artists with less than 5 songs
        var tracksToKeep = artistsSummary
            .Where(artistSummary => artistSummary.Count <= 5)
            .SelectMany(artistSummary => tracks.Where(track => track.Track.Artists[0].Id == artistSummary.Id))
            .ToList();
        
        // Convert tracks to IDs
        var trackIDs = TrackUtils.ConvertTracksToIds(tracksToKeep);

        // Insert tracks into playlist with pagination
        var offset = Constants.Offset;
        var limit = Constants.LimitInsertPlaylistTracks;

        while (true)
        {
            var tracksToAdd = trackIDs.Skip(offset).Take(limit).ToList();

            var added = await Client.Spotify.Playlists.AddItems(idPlaylistMinorSongs!,
                new PlaylistAddItemsRequest(tracksToAdd));

            if (added.SnapshotId == string.Empty)
            {
                return false;
            }

            if (tracksToAdd.Count < limit)
            {
                break;
            }

            offset += limit;
        }

        return true;
    }
}