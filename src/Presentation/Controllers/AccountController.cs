using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using tracksByPopularity.Application.DTOs;
using tracksByPopularity.Application.Services;

namespace tracksByPopularity.Presentation.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController : ControllerBase
{
    private readonly IAccountAuthService _authService;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IAccountAuthService authService, ILogger<AccountController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request.Email, request.Password);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Registration successful. Please check your email to verify your account." }));
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse>> Login([FromBody] AccountLoginRequest request)
    {
        var result = await _authService.LoginAsync(request.Email, request.Password);

        if (!result.Success)
        {
            return Unauthorized(ApiResponse.Fail(result.Error!));
        }

        var userDto = new UserDto
        {
            Id = result.User!.Id,
            Email = result.User.Email,
            IsEmailVerified = result.User.IsEmailVerified,
            IsSpotifyLinked = result.User.SpotifyLink != null
        };

        Response.Cookies.Append("access_token", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(7)
        });

        return Ok(ApiResponse.Ok(new LoginResponse
        {
            Token = result.Token!,
            User = userDto
        }));
    }

    [HttpGet("verify/{token}")]
    public async Task<ActionResult<ApiResponse>> VerifyEmail(string token)
    {
        var result = await _authService.VerifyEmailAsync(token);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Email verified successfully. You can now log in." }));
    }

    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request.Email);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "If an account with that email exists, a password reset link has been sent." }));
    }

    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Password reset successful. You can now log in with your new password." }));
    }

    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var result = await _authService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Password changed successfully." }));
    }

    [Authorize]
    [HttpPost("link-spotify")]
    public async Task<ActionResult<ApiResponse>> LinkSpotify([FromBody] LinkSpotifyRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var result = await _authService.LinkSpotifyAsync(userId.Value, request.SpotifyUserId, request.AccessToken, request.RefreshToken, request.TokenExpiresAt);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        Response.Cookies.Append("access_token", result.Token!, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            MaxAge = TimeSpan.FromDays(7)
        });

        return Ok(ApiResponse.Ok(new { message = "Spotify account linked successfully.", token = result.Token }));
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var user = await _authService.GetUserByIdAsync(userId.Value);
        if (user == null)
        {
            return NotFound(ApiResponse.Fail("User not found"));
        }

        var userDto = new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            IsEmailVerified = user.IsEmailVerified,
            IsSpotifyLinked = user.SpotifyLink != null
        };

        return Ok(ApiResponse.Ok(userDto));
    }

    [Authorize]
    [HttpPost("logout")]
    public ActionResult<ApiResponse> Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok(ApiResponse.Ok(new { message = "Logged out successfully." }));
    }

    private Guid? GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return null;
        }
        return userId;
    }
}
