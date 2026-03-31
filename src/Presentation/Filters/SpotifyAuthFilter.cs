using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace tracksByPopularity.Presentation.Filters;

/// <summary>
/// Action filter that validates Spotify authentication via header or cookie and resolves the SpotifyClient.
/// Apply [SpotifyAuth] to controllers or actions that require an authenticated Spotify user.
/// Priority: X-Spotify-User-Id header > spotify_user_id cookie.
/// </summary>
public class SpotifyAuthFilter(SpotifyAuthService spotifyAuthService) : IAsyncActionFilter
{
    public const string UserIdCookieName = "spotify_user_id";
    public const string UserIdHeaderName = "X-Spotify-User-Id";
    public const string SpotifyClientKey = "SpotifyClient";
    public const string SpotifyUserIdKey = "SpotifyUserId";

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = GetUserId(context.HttpContext.Request);

        if (string.IsNullOrEmpty(userId))
        {
            context.Result = new UnauthorizedObjectResult(
                ApiResponse.Fail("Not authenticated. Please log in with Spotify."));
            return;
        }

        var spotifyClient = await spotifyAuthService.GetSpotifyClientForUserAsync(userId);

        context.HttpContext.Items[SpotifyUserIdKey] = userId;
        context.HttpContext.Items[SpotifyClientKey] = spotifyClient;

        await next();
    }

    /// <summary>
    /// Resolves the Spotify user ID from the request, checking header first then cookie.
    /// </summary>
    public static string? GetUserId(HttpRequest request)
    {
        var userId = request.Headers[UserIdHeaderName].FirstOrDefault();
        if (!string.IsNullOrEmpty(userId))
            return userId;

        return request.Cookies[UserIdCookieName];
    }
}

/// <summary>
/// Attribute to apply Spotify authentication filter to controllers or actions.
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class SpotifyAuthAttribute() : TypeFilterAttribute(typeof(SpotifyAuthFilter));
