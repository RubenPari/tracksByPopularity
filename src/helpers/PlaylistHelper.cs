using SpotifyAPI.Web;

namespace tracksByPopularity.helpers;

public static class PlaylistHelper
{
    public static async Task<Dictionary<string, string>> GetOrCreateArtistPlaylists(
        string artistId,
        SpotifyClient spotifyClient
    )
    {
        var artistPlaylistsId = new Dictionary<string, string>();
        var userId = (await spotifyClient.UserProfile.Current()).Id;
        var artistName = (await spotifyClient.Artists.Get(artistId)).Name;

        // Retrieve all user playlists
        var pagingUserPlaylists = await spotifyClient.Playlists.GetUsers(userId);
        var userPlaylists = await spotifyClient.PaginateAll(pagingUserPlaylists);

        // Check if artist playlists already exist (less-medium-more)
        foreach (var userPlaylist in userPlaylists)
        {
            if (userPlaylist.Name == $"{artistName} less")
            {
                artistPlaylistsId["less"] = userPlaylist.Id!;
            }
            else if (userPlaylist.Name == $"{artistName} medium")
            {
                artistPlaylistsId["medium"] = userPlaylist.Id!;
            }
            else if (userPlaylist.Name == $"{artistName} more")
            {
                artistPlaylistsId["more"] = userPlaylist.Id!;
            }
        }

        if (artistPlaylistsId.Count == 3)
        {
            return artistPlaylistsId;
        }

        // Create playlists if they don't already exist
        artistPlaylistsId.Clear();

        artistPlaylistsId["less"] = (
            await spotifyClient.Playlists.Create(
                userId,
                new PlaylistCreateRequest($"{artistName} less")
            )
        ).Id!;

        artistPlaylistsId["medium"] = (
            await spotifyClient.Playlists.Create(
                userId,
                new PlaylistCreateRequest($"{artistName} medium")
            )
        ).Id!;

        artistPlaylistsId["more"] = (
            await spotifyClient.Playlists.Create(
                userId,
                new PlaylistCreateRequest($"{artistName} more")
            )
        ).Id!;

        return artistPlaylistsId;
    }
}
