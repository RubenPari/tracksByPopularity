using SpotifyAPI.Web;

namespace tracksByPopularity.services.Spotify;

public class SpotifyService : ISpotifyService
{
    private readonly IAuthService _authService;
    private SpotifyClient? _spotifyClient;

    public SpotifyService(IAuthService authService)
    {
        _authService = authService;
        InitializeSpotifyClient().Wait();
    }

    private async Task InitializeSpotifyClient()
    {
        var token = await _authService.GetAccessTokenAsync();
        _spotifyClient = new SpotifyClient(token);
    }

    public async Task<string> GetAccessTokenAsync()
    {
        return await _authService.GetAccessTokenAsync();
    }
}