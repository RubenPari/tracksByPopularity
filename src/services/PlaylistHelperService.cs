using SpotifyAPI.Web;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for playlist helper operations.
/// Handles creation and retrieval of artist-specific playlists organized by popularity.
/// </summary>
public class PlaylistHelperService : IPlaylistHelper
{
    /// <summary>
    /// Retrieves or creates three playlists for a specific artist, organized by track popularity.
    /// The playlists are named "{artistName} less", "{artistName} medium", and "{artistName} more".
    /// </summary>
    /// <param name="spotifyClient">The authenticated Spotify client instance.</param>
    /// <param name="artistId">The unique identifier of the artist.</param>
    /// <returns>
    /// A dictionary with keys "less", "medium", and "more", each containing the corresponding playlist ID.
    /// </returns>
    /// <remarks>
    /// This method:
    /// 1. Retrieves the current user's ID and artist name
    /// 2. Searches all user playlists for existing artist playlists
    /// 3. If all three playlists exist, returns their IDs immediately
    /// 4. If any are missing, creates all three playlists (to ensure consistency)
    /// 5. Returns the dictionary with all three playlist IDs
    /// </remarks>
    public async Task<Dictionary<string, string>> GetOrCreateArtistPlaylistsAsync(
        SpotifyClient spotifyClient,
        string artistId
    )
    {
        var artistPlaylistsId = new Dictionary<string, string>();
        var userId = (await spotifyClient.UserProfile.Current()).Id;
        var artistName = (await spotifyClient.Artists.Get(artistId)).Name;

        // Retrieve all user playlists
        var pagingUserPlaylists = await spotifyClient.Playlists.GetUsers(userId);
        var userPlaylists = await spotifyClient.PaginateAll(pagingUserPlaylists);

        // Search for existing artist playlists (less-medium-more)
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

        // If all three playlists exist, return them
        if (artistPlaylistsId.Count == 3)
        {
            return artistPlaylistsId;
        }

        // Create all three playlists if any are missing (ensures consistency)
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

