using System.Security.Claims;
using SpotifyAPI.Web;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

public static class AuthController
{
    public static IResult Login()
    {
        var request = new LoginRequest(
            new Uri(Constants.RedirectUri),
            Constants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Constants.MyScopes,
        };

        var uri = request.ToUri();

        return Results.Redirect(uri.ToString());
    }

    public static async Task<IResult> Callback(string code, SpotifyAuthService spotifyAuthService)
    {
        try
        {
            var response = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    Constants.ClientId,
                    Constants.ClientSecret,
                    code,
                    new Uri(Constants.RedirectUri)
                )
            );

            var spotifyClient = new SpotifyClient(Constants.Config.WithToken(response.AccessToken));
            var user = await spotifyClient.UserProfile.Current();

            if (string.IsNullOrEmpty(user.Id))
            {
                return Results.BadRequest("Login failed, retry");
            }

            await spotifyAuthService.StoreTokenAsync(response, user.Id);

            // Redirect to a page where the user can generate an API key
            return Results.Ok(
                new
                {
                    message = "Authentication successful! Now generate an API key to access the API.",
                    userId = user.Id,
                    generateApiKeyUrl = "/auth/api-key?description=Initial API Key"
                }
            );
        }
        catch (Exception ex)
        {
            return Results.Problem($"Authentication error: {ex.Message}");
        }
    }

    public static async Task<IResult> Logout(
        HttpContext context,
        ApiKeyService apiKeyService,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            // Get current user ID from the context
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.BadRequest("Not authenticated");
            }

            // Get the current API key from the header
            if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyValue))
            {
                return Results.BadRequest("No API key provided");
            }

            // Option 1: Revoke only the current API key
            await apiKeyService.RevokeApiKeyAsync(apiKeyValue!);

            // Option 2: Remove the Spotify tokens for this user (uncomment if needed)
            // await spotifyAuthService.RemoveTokenAsync(userId);

            return Results.Ok(new { message = "Successfully logged out" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Logout error: {ex.Message}");
        }
    }

    public static async Task<IResult> LogoutAll(
        HttpContext context,
        ApiKeyService apiKeyService,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            // Get current user ID from the context
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.BadRequest("Not authenticated");
            }

            // Revoke all API keys for this user
            var keys = await apiKeyService.GetUserApiKeysAsync(userId);
            foreach (var key in keys)
            {
                await apiKeyService.RevokeApiKeyAsync(key.Key);
            }

            // Remove the Spotify tokens for this user
            await spotifyAuthService.RemoveTokenAsync(userId);

            return Results.Ok(new { message = "Successfully logged out from all devices" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Logout error: {ex.Message}");
        }
    }
}
