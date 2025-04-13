using SpotifyAPI.Web;
using tracksByPopularity.models;

namespace tracksByPopularity.utils;

public class SpotifyClientAccessor(
    IHttpContextAccessor httpContextAccessor,
    ITokenService tokenService
) : ISpotifyClientAccessor
{
    public SpotifyClient GetClient()
    {
        var httpContext =
            httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("HttpContext is not available");

        var authHeader = httpContext
            .Request.Headers.Authorization.FirstOrDefault()
            ?.Replace("Bearer ", "");

        if (string.IsNullOrEmpty(authHeader))
        {
            throw new UnauthorizedAccessException("Authorization header is missing");
        }

        var userToken = tokenService.ValidateToken(authHeader);
        if (userToken == null)
        {
            throw new UnauthorizedAccessException("Invalid token");
        }

        // Check if token has expired
        if (userToken.ExpiresAt < DateTime.UtcNow)
        {
            throw new UnauthorizedAccessException("Spotify token has expired, please login again");
        }

        return tokenService.CreateSpotifyClient(userToken.AccessToken);
    }
}
