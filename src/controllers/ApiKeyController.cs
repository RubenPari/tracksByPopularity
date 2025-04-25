using System.Security.Claims;
using SpotifyAPI.Web;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

public static class ApiKeyController
{
    public static async Task<IResult> GenerateApiKey(
        string description,
        ApiKeyService apiKeyService,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            // Since we're in the auth process already, we can get the current user
            var spotifyClient = new SpotifyClient(Constants.Config);
            var user = await spotifyClient.UserProfile.Current();

            if (string.IsNullOrEmpty(user.Id))
            {
                return Results.BadRequest("Cannot generate API key: User not found");
            }

            var apiKey = await apiKeyService.GenerateApiKeyAsync(user.Id, description);

            return Results.Ok(
                new
                {
                    key = apiKey.Key,
                    expiresAt = apiKey.ExpiresAt,
                    message = "Store this API key securely. It won't be shown again."
                }
            );
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error generating API key: {ex.Message}");
        }
    }

    public static async Task<IResult> ListApiKeys(
        HttpContext context,
        ApiKeyService apiKeyService,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            // Get current user
            var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Results.Unauthorized();
            }

            var keys = await apiKeyService.GetUserApiKeysAsync(userId);

            // Don't expose the actual key value in the list, only metadata
            var keyInfo = keys.Select(k => new
            {
                id = string.Concat(k.Key.AsSpan(0, 8), "..."),
                description = k.Description,
                createdAt = k.CreatedAt,
                expiresAt = k.ExpiresAt
            });

            return Results.Ok(keyInfo);
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error listing API keys: {ex.Message}");
        }
    }

    public static async Task<IResult> RevokeApiKey(string keyId, ApiKeyService apiKeyService)
    {
        try
        {
            await apiKeyService.RevokeApiKeyAsync(keyId);
            return Results.Ok(new { message = "API key revoked successfully" });
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error revoking API key: {ex.Message}");
        }
    }
}
