using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Infrastructure.Helpers;
using tracksByPopularity.Infrastructure.Services;

namespace tracksByPopularity.Presentation.Controllers;

/// <summary>
/// API controller for Spotify OAuth authentication.
/// Handles login, callback, session check, and logout.
/// </summary>
[ApiController]
[Route("auth")]
public class AuthController : ControllerBase
{
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly ILogger<AuthController> _logger;
    private const string UserIdCookieName = "spotify_user_id";

    /// <summary>
    /// The frontend origin URL. In Docker, the frontend runs on port 80 via nginx,
    /// while the backend is on port 8080. The OAuth callback hits the backend directly,
    /// so redirects after auth must go to the frontend origin.
    /// </summary>
    private static readonly string FrontendOrigin =
        Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "http://127.0.0.1";

    public AuthController(
        SpotifyAuthService spotifyAuthService,
        ILogger<AuthController> logger
    )
    {
        _spotifyAuthService = spotifyAuthService;
        _logger = logger;
    }

    /// <summary>
    /// Returns the Spotify authorization URL for the user to log in.
    /// </summary>
    [HttpGet("login")]
    public ActionResult<ApiResponse<object>> Login()
    {
        var loginRequest = new LoginRequest(
            new Uri(Constants.RedirectUri),
            Constants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Constants.MyScopes,
        };

        var loginUrl = loginRequest.ToUri().ToString();
        _logger.LogInformation("Generated Spotify login URL");

        return Ok(ApiResponse<object>.Ok(new { loginUrl }));
    }

    /// <summary>
    /// Handles the Spotify OAuth callback.
    /// Exchanges the authorization code for tokens and stores them in Redis.
    /// </summary>
    [HttpGet("callback")]
    public async Task<ActionResult> Callback([FromQuery] string? code, [FromQuery] string? error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Spotify auth denied: {Error}", error);
            return Redirect($"{FrontendOrigin}/?error=auth_denied");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("No code received in callback");
            return Redirect($"{FrontendOrigin}/?error=no_code");
        }

        try
        {
            // Exchange code for tokens
            var tokenResponse = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    Constants.ClientId,
                    Constants.ClientSecret,
                    code,
                    new Uri(Constants.RedirectUri)
                )
            );

            // Create a temporary client to get the user's ID
            var spotifyClient = new SpotifyClient(tokenResponse.AccessToken);
            var profile = await spotifyClient.UserProfile.Current();
            var userId = profile.Id;

            // Store token in Redis
            await _spotifyAuthService.StoreTokenAsync(tokenResponse, userId);

            // Set user ID cookie so subsequent requests know which user this is
            Response.Cookies.Append(UserIdCookieName, userId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false, // Set to true in production with HTTPS
                MaxAge = TimeSpan.FromDays(30),
                Path = "/",
            });

            _logger.LogInformation("Successfully authenticated user: {UserId}", userId);
            return Redirect(FrontendOrigin);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during OAuth callback");
            return Redirect($"{FrontendOrigin}/?error=auth_failed");
        }
    }

    /// <summary>
    /// Checks if the current user is authenticated.
    /// </summary>
    [HttpGet("is-auth")]
    public async Task<ActionResult<ApiResponse<object>>> IsAuthenticated()
    {
        var userId = Request.Cookies[UserIdCookieName];

        if (string.IsNullOrEmpty(userId))
        {
            return Ok(ApiResponse<object>.Ok(new { authenticated = false }));
        }

        try
        {
            // Verify the token is still valid by attempting to create a client
            var client = await _spotifyAuthService.GetSpotifyClientForUserAsync(userId);
            return Ok(ApiResponse<object>.Ok(new { authenticated = true, userId }));
        }
        catch (UnauthorizedAccessException)
        {
            // Token expired or not found — clear cookie
            Response.Cookies.Delete(UserIdCookieName);
            return Ok(ApiResponse<object>.Ok(new { authenticated = false }));
        }
    }

    /// <summary>
    /// Logs the user out by removing their token and cookie.
    /// </summary>
    [HttpPost("logout")]
    public async Task<ActionResult<ApiResponse>> Logout()
    {
        var userId = Request.Cookies[UserIdCookieName];

        if (!string.IsNullOrEmpty(userId))
        {
            await _spotifyAuthService.RemoveTokenAsync(userId);
        }

        Response.Cookies.Delete(UserIdCookieName);
        _logger.LogInformation("User logged out: {UserId}", userId ?? "unknown");

        return Ok(ApiResponse.Ok("Logged out successfully"));
    }
}
