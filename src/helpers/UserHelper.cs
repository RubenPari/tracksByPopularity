using SpotifyAPI.Web;

namespace tracksByPopularity.helpers;

public static class UserHelper
{
    public static async Task<string> GetUserId(SpotifyClient spotifyClient)
    {
        var currentUser = await spotifyClient.UserProfile.Current();
        return currentUser.Id;
    }
}
