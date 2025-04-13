using SpotifyAPI.Web;

namespace tracksByPopularity.models;

public interface ITokenService
{
    string GenerateJwtToken(string userId, string spotifyAccessToken, DateTime expiresAt);
    UserToken? ValidateToken(string token);
    SpotifyClient CreateSpotifyClient(string accessToken);
}
