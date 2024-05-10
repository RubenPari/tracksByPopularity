using SpotifyAPI.Web;

namespace tracksByPopularity.services;

public static class TrackService
{
    public static async Task GetAllUserTracks()
    {
        var config = SpotifyClientConfig.CreateDefault();
        
        var request = new ClientCredentialsRequest("CLIENT_ID", "CLIENT_SECRET");
        var response = await new OAuthClient(config).RequestToken(request);

        var spotify = new SpotifyClient(config.WithToken(response.AccessToken));
    }
}