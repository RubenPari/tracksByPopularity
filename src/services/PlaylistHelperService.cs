using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public class PlaylistHelperService : IPlaylistHelper
{
    public async Task<Dictionary<string, string>> GetOrCreateArtistPlaylistsAsync(
        SpotifyClient spotifyClient,
        string artistId
    )
    {
        var artistPlaylistsId = new Dictionary<string, string>();
        var userId = (await spotifyClient.UserProfile.Current()).Id;
        var artistName = (await spotifyClient.Artists.Get(artistId)).Name;

        // Recupera tutte le playlist dell'utente
        var pagingUserPlaylists = await spotifyClient.Playlists.GetUsers(userId);
        var userPlaylists = await spotifyClient.PaginateAll(pagingUserPlaylists);

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

