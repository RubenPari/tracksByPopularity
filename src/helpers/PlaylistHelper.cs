namespace tracksByPopularity.src.helpers;

public static class PlaylistHelper
{
    public static async Task<bool> CheckValidityPlaylist(params string[] playlistIds)
    {
        foreach (var playlistId in playlistIds)
        {
            var playlist = await Client.Spotify!.Playlists.Get(playlistId);
            if (playlist.Id is null)
            {
                return false;
            }
        }

        return true;
    }
}
