using SpotifyAPI.Web;

namespace tracksByPopularity.helpers;

public abstract class UserHelper
{
    public static async Task<string> GetUserIdFromClient(SpotifyClient spotifyClient)
    {
        var currentUser = await spotifyClient.UserProfile.Current();
        return currentUser.Id;
    }
}
