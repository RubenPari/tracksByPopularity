using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SpotifyAPI.Web;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Services;
using tracksByPopularity.Infrastructure.Helpers;
using tracksByPopularity.Infrastructure.Services;

namespace tracksByPopularity.Presentation.Controllers;

[ApiController]
[Route("api/spotify")]
public class SpotifyLinkController : ControllerBase
{
    private readonly SpotifyAuthService _spotifyAuthService;
    private readonly IAccountAuthService _accountAuthService;
    private readonly IJwtService _jwtService;
    private readonly ILogger<SpotifyLinkController> _logger;
    private const string UserIdCookieName = "spotify_user_id";

    private static readonly string FrontendOrigin =
        Environment.GetEnvironmentVariable("FRONTEND_ORIGIN") ?? "http://127.0.0.1:5173";

    public SpotifyLinkController(
        SpotifyAuthService spotifyAuthService,
        IAccountAuthService accountAuthService,
        IJwtService jwtService,
        ILogger<SpotifyLinkController> logger)
    {
        _spotifyAuthService = spotifyAuthService;
        _accountAuthService = accountAuthService;
        _jwtService = jwtService;
        _logger = logger;
    }

    [HttpGet("link-url")]
    [Authorize]
    public ActionResult<ApiResponse> GetLinkUrl()
    {
        var loginRequest = new LoginRequest(
            new Uri($"{Constants.RedirectUri}/api/spotify/callback"),
            Constants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Constants.MyScopes,
            State = "link_spotify"
        };

        var loginUrl = loginRequest.ToUri().ToString();

        return Ok(ApiResponse.Ok(new { linkUrl = loginUrl }));
    }

    [HttpGet("callback")]
    public async Task<ActionResult> Callback([FromQuery] string? code, [FromQuery] string? error, [FromQuery] string? state)
    {
        if (!string.IsNullOrEmpty(error))
        {
            _logger.LogWarning("Spotify auth denied during link: {Error}", error);
            return Redirect($"{FrontendOrigin}/settings?error=auth_denied");
        }

        if (string.IsNullOrEmpty(code))
        {
            _logger.LogWarning("No code received in link callback");
            return Redirect($"{FrontendOrigin}/settings?error=no_code");
        }

        if (state != "link_spotify")
        {
            _logger.LogWarning("Invalid state in link callback");
            return Redirect($"{FrontendOrigin}/settings?error=invalid_state");
        }

        try
        {
            var tokenResponse = await new OAuthClient().RequestToken(
                new AuthorizationCodeTokenRequest(
                    Constants.ClientId,
                    Constants.ClientSecret,
                    code,
                    new Uri($"{Constants.RedirectUri}/api/spotify/callback")
                )
            );

            var spotifyClient = new SpotifyClient(tokenResponse.AccessToken);
            var profile = await spotifyClient.UserProfile.Current();
            var spotifyUserId = profile.Id;

            var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                return Redirect($"{FrontendOrigin}/settings?error=not_authenticated");
            }

            var linkResult = await _accountAuthService.LinkSpotifyAsync(
                userId,
                spotifyUserId,
                tokenResponse.AccessToken,
                tokenResponse.RefreshToken,
                DateTime.UtcNow.AddSeconds(tokenResponse.ExpiresIn)
            );

            if (!linkResult.Success)
            {
                return Redirect($"{FrontendOrigin}/settings?error={Uri.EscapeDataString(linkResult.Error!)}");
            }

            await _spotifyAuthService.StoreTokenAsync(tokenResponse, spotifyUserId);

            Response.Cookies.Append(UserIdCookieName, spotifyUserId, new CookieOptions
            {
                HttpOnly = true,
                SameSite = SameSiteMode.Lax,
                Secure = false,
                MaxAge = TimeSpan.FromDays(30),
                Path = "/"
            });

            return Redirect($"{FrontendOrigin}/settings?success=spotify_linked");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during Spotify link callback");
            return Redirect($"{FrontendOrigin}/settings?error=link_failed");
        }
    }

    [Authorize]
    [HttpGet("status")]
    public async Task<ActionResult<ApiResponse>> GetLinkStatus()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Ok(ApiResponse.Ok(new SpotifyLinkStatusDto { IsLinked = false }));
        }

        var spotifyLink = await _accountAuthService.GetSpotifyLinkAsync(userId);

        return Ok(ApiResponse.Ok(new SpotifyLinkStatusDto
        {
            IsLinked = spotifyLink != null,
            SpotifyUserId = spotifyLink?.SpotifyUserId
        }));
    }

    [Authorize]
    [HttpPost("unlink")]
    public async Task<ActionResult<ApiResponse>> Unlink()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var spotifyLink = await _accountAuthService.GetSpotifyLinkAsync(userId);
        if (spotifyLink != null)
        {
            await _spotifyAuthService.RemoveTokenAsync(spotifyLink.SpotifyUserId);
            await _accountAuthService.UnlinkSpotifyAsync(userId);
        }

        return Ok(ApiResponse.Ok(new { message = "Spotify account unlinked successfully." }));
    }
}
