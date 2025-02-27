using SpotifyAPI.Web;
using tracksByPopularity.utils;

namespace tracksByPopularity.helpers;

public static class PlaylistHelper
{
    public static async Task<Dictionary<string, string>> GetOrCreateArtistPlaylists(string artistId)
    {
        var artistPlaylistsId = new Dictionary<string, string>();
        var userId = (await Client.Spotify!.UserProfile.Current()).Id;
        var artistName = (await Client.Spotify.Artists.Get(artistId)).Name;

        // Recupera tutte le playlist dell'utente
        var pagingUserPlaylists = await Client.Spotify.Playlists.GetUsers(userId);
        var userPlaylists = await Client.Spotify.PaginateAll(pagingUserPlaylists);

        // Cerca se esistono già le playlist dell'artista (less-medium-more)
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

        // Crea le playlist se non esistono già
        artistPlaylistsId.Clear();

        artistPlaylistsId["less"] = (
            await Client.Spotify.Playlists.Create(
                userId,
                new PlaylistCreateRequest($"{artistName} less")
            )
        ).Id!;

        artistPlaylistsId["medium"] = (
            await Client.Spotify.Playlists.Create(
                userId,
                new PlaylistCreateRequest($"{artistName} medium")
            )
        ).Id!;

        artistPlaylistsId["more"] = (
            await Client.Spotify.Playlists.Create(
                userId,
                new PlaylistCreateRequest($"{artistName} more")
            )
        ).Id!;

        return artistPlaylistsId;
    }
}
