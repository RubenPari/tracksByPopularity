using tracksByPopularity.utils;

namespace tracksByPopularity.helpers;

public abstract class UserHelper
{
    public static async Task<string> GetUserId()
    {
        var currentUser = await Client.Spotify!.UserProfile.Current();

        return currentUser.Id;
    }
}
