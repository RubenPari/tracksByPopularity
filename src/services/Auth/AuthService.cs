using Microsoft.Extensions.Options;

namespace tracksByPopularity.services;

public class AuthService(IOptions<SpotifyAuthOptions> options) : IAuthService
{
    private readonly IOptions<SpotifyAuthOptions> _options = options;

    public Task<string> GetAccessTokenAsync()
    {
        return Task.FromResult("access_token");
    }

    public void Logout()
    {
    }
}