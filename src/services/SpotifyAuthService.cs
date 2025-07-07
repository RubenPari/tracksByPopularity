using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.utils;

namespace tracksByPopularity.services;

public class SpotifyAuthService(IConnectionMultiplexer redis)
{
    public static SpotifyClient GetSpotifyClientAsync()
    {
        // Use client credentials flow for public API access
        var config = SpotifyClientConfig
            .CreateDefault()
            .WithAuthenticator(
                new ClientCredentialsAuthenticator(Constants.ClientId, Constants.ClientSecret)
            );

        return new SpotifyClient(config);
    }

    public async Task<SpotifyClient> GetSpotifyClientForUserAsync(string userId)
    {
        var db = redis.GetDatabase();
        var tokenJson = await db.StringGetAsync($"spotify_token:{userId}");

        if (!tokenJson.HasValue)
        {
            throw new UnauthorizedAccessException("Spotify token not found for user");
        }

        var token = JsonConvert.DeserializeObject<TokenData>(tokenJson!);

        // Check if token is expired (with 5 minutes buffer)
        if (token!.CreatedAt.AddSeconds(token.ExpiresIn - 300) < DateTime.UtcNow)
        {
            // Token is expired, refresh it
            token = await RefreshTokenAsync(token.RefreshToken, userId);
        }

        return new SpotifyClient(Constants.Config.WithToken(token.AccessToken!));
    }

    public async Task StoreTokenAsync(AuthorizationCodeTokenResponse response, string userId)
    {
        var tokenData = new TokenData
        {
            AccessToken = response.AccessToken,
            TokenType = response.TokenType,
            ExpiresIn = response.ExpiresIn,
            RefreshToken = response.RefreshToken,
            Scope = response.Scope,
            CreatedAt = DateTime.UtcNow
        };

        var db = redis.GetDatabase();

        // Store token in Redis with user's ID as key
        await db.StringSetAsync(
            $"spotify_token:{userId}",
            JsonConvert.SerializeObject(tokenData),
            TimeSpan.FromDays(30) // TTL for token
        );
    }

    public async Task RemoveTokenAsync(string userId)
    {
        var db = redis.GetDatabase();
        await db.KeyDeleteAsync($"spotify_token:{userId}");
    }

    private async Task<TokenData> RefreshTokenAsync(string? refreshToken, string userId)
    {
        var newToken = await new OAuthClient().RequestToken(
            new AuthorizationCodeRefreshRequest(
                Constants.ClientId,
                Constants.ClientSecret,
                refreshToken!
            )
        );

        var tokenData = new TokenData
        {
            AccessToken = newToken.AccessToken,
            TokenType = newToken.TokenType,
            ExpiresIn = newToken.ExpiresIn,
            RefreshToken = refreshToken, // Preserve the refresh token as it might not be returned
            Scope = newToken.Scope,
            CreatedAt = DateTime.UtcNow
        };

        var db = redis.GetDatabase();
        await db.StringSetAsync(
            $"spotify_token:{userId}",
            JsonConvert.SerializeObject(tokenData),
            TimeSpan.FromDays(30)
        );

        return tokenData;
    }
}

public class TokenData
{
    public string? AccessToken { get; init; }
    public string? TokenType { get; set; }
    public int ExpiresIn { get; init; }
    public string? RefreshToken { get; init; }
    public string? Scope { get; set; }
    public DateTime CreatedAt { get; init; }
}
