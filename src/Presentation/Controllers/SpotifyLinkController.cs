using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SpotifyAPI.Web;
using tracksByPopularity.Presentation.Filters;

namespace tracksByPopularity.Presentation.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyLinkController(
    SpotifyAuthService spotifyAuthService,
    IAccountAuthService accountAuthService,
    ILogger<SpotifyLinkController> logger,
    IOptions<AppSettings> appSettings,
    IOptions<SpotifySettings> spotifySettings)
    : ControllerBase
{
    private readonly AppSettings _appSettings = appSettings.Value;
    private readonly SpotifySettings _spotifySettings = spotifySettings.Value;

    /// <summary>
    /// Get the URL to link a Spotify account.
    /// </summary>
    [HttpGet("link-url")]
    [Authorize]
    public ActionResult<ApiResponse> GetLinkUrl()
    {
        var loginRequest = new LoginRequest(
            new Uri($"{_spotifySettings.RedirectUri}/api/spotify/callback"),
            _spotifySettings.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Constants.MyScopes,
            State = "link_spotify" // TODO: Use a random string
        };

        var loginUrl = loginRequest.ToUri().ToString();

        return Ok(ApiResponse.Ok(new { linkUrl = loginUrl }));
    }

    /// <summary>
    /// Callback for the Spotify OAuth flow.
    /// </summary>
    [HttpGet("callback")]
    public async Task<ActionResult> Callback([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? state)
    {
        if (!string.IsNullOrEmpty(error))
        {
            logger.LogWarning("Spotify auth denied during link: {Error}", error);
            return Redirect($"{_appSettings.FrontendOrigin}/settings?error=auth_denied");
        }

        if (string.IsNullOrEmpty(code))
        {
            logger.LogWarning("No code received in link callback");
            return Redirect($"{_appSettings.FrontendOrigin}/settings?error=no_code");
        }

        if (state != "link_spotify")
        {
            logger.LogWarning("Invalid state in link callback");
            return Redirect($"{_appSettings.FrontendOrigin}/settings?error=invalid_state");
        }

        try
        {
            var tokenResponse = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    _spotifySettings.ClientId,
                    _spotifySettings.ClientSecret,
                    code,
                    new Uri($"{_spotifySettings.RedirectUri}/api/spotify/callback")
                )
            );

            var spotifyClient = new SpotifyClient(tokenResponse.AccessToken);
            var profile = await spotifyClient.UserProfile.Current();
            var spotifyUserId = profile.Id;

            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Redirect($"{_appSettings.FrontendOrigin}/settings?error=not_authenticated");
            }

            var linkResult = await accountAuthService.LinkSpotifyAsync(
                userId,
                spotifyUserId,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
            );

            if (!linkResult.Success)
            {
                return Redirect($"{_appSettings.FrontendOrigin}/settings?error={Uri.EscapeDataString(linkResult.Error!)}");
            }

            await spotifyAuthService.StoreTokenAsync(tokenResponse, spotifyUserId);

            Response.Cookies.Append(SpotifyAuthFilter.UserIdCookieName, spotifyUserId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                MaxAge = TimeSpan.FromDays(30),
                Path = "/"
            });

            return Redirect($"{_appSettings.FrontendOrigin}/settings?success=spotify_linked");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during Spotify link callback");
            return Redirect($"{_appSettings.FrontendOrigin}/settings?error=link_failed");
        }
    }

    /// <summary>
    /// Gets the current Spotify link status for the authenticated user.
    /// </summary>
    [Authorize]
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse>> GetLinkStatus()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Ok(ApiResponse.Ok(new SpotifyLinkStatusDto { IsLinked = false }));
        }

        var spotifyLink = await accountAuthService.GetSpotifyLinkAsync(userId);

        return Ok(ApiResponse.Ok(new SpotifyLinkStatusDto
        {
            IsLinked = spotifyLink != null,
            SpotifyUserId = spotifyLink?.SpotifyUserId
        }));
    }

    /// <summary>
    /// Unlinks the Spotify account from the user's account.
    /// </summary>
    [Authorize]
    [HttpPost("unlink")]
    public async Task<ActionResult<ApiResponse>> Unlink()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var spotifyLink = await accountAuthService.GetSpotifyLinkAsync(userId);
        
        if (spotifyLink == null) return Ok(ApiResponse.Ok(new { message = "Spotify account unlinked successfully." }));
        
        await spotifyAuthService.RemoveTokenAsync(spotifyLink.SpotifyUserId);
        await accountAuthService.UnlinkSpotifyAsync(userId);

        return Ok(ApiResponse.Ok(new { message = "Spotify account unlinked successfully." }));
    }
}
