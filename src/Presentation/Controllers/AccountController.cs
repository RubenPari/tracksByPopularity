using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace tracksByPopularity.Presentation.Controllers;

[ApiController]
[Route("api/account")]
public class AccountController(IAccountAuthService authService) : ControllerBase
{
    /// <summary>
    /// Registers a new user
    /// </summary>
    /// <param name="request">The registration request</param>
    /// <returns>The result of the registration</returns>
    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await authService.RegisterAsync(request.Email, request.Password);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Registration successful. Please check your email to verify your account." }));
    }

    /// <summary>
    /// Logs in a user
    /// </summary>
    /// <param name="request">The login request</param>
    /// <returns>The result of the login</returns>
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse>> Login([FromBody] AccountLoginRequest request)
    {
        var result = await authService.LoginAsync(request.Email, request.Password);

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

    /// <summary>
    /// Verifies a user's email
    /// </summary>
    /// <param name="token">The verification token</param>
    /// <returns>The result of the verification</returns>
    [HttpGet("verify/{token}")]
    public async Task<ActionResult<ApiResponse>> VerifyEmail(string token)
    {
        var result = await authService.VerifyEmailAsync(token);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Email verified successfully. You can now log in." }));
    }

    /// <summary>
    /// Sends a password reset email
    /// </summary>
    /// <param name="request">The forgot password request</param>
    /// <returns>The result of the request</returns>
    [HttpPost("forgot-password")]
    public async Task<ActionResult<ApiResponse>> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await authService.ForgotPasswordAsync(request.Email);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "If an account with that email exists, a password reset link has been sent." }));
    }

    /// <summary>
    /// Resets a user's password
    /// </summary>
    /// <param name="request">The reset password request</param>
    /// <returns>The result of the reset</returns>
    [HttpPost("reset-password")]
    public async Task<ActionResult<ApiResponse>> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await authService.ResetPasswordAsync(request.Token, request.NewPassword);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Password reset successful. You can now log in with your new password." }));
    }

    /// <summary>
    /// Changes a user's password
    /// </summary>
    /// <param name="request">The change password request</param>
    /// <returns>The result of the change</returns>
    [Authorize]
    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var result = await authService.ChangePasswordAsync(userId.Value, request.OldPassword, request.NewPassword);

        if (!result.Success)
        {
            return BadRequest(ApiResponse.Fail(result.Error!));
        }

        return Ok(ApiResponse.Ok(new { message = "Password changed successfully." }));
    }

    /// <summary>
    /// Links a Spotify account to a user
    /// </summary>
    /// <param name="request">The link Spotify request</param>
    /// <returns>The result of the linking</returns>
    [Authorize]
    [HttpPost("link-spotify")]
    public async Task<ActionResult<ApiResponse>> LinkSpotify([FromBody] LinkSpotifyRequest request)
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var result = await authService.LinkSpotifyAsync(userId.Value, request.SpotifyUserId, request.AccessToken, request.RefreshToken, request.TokenExpiresAt);

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

    /// <summary>
    /// Gets the current user's information
    /// </summary>
    /// <returns>The current user's information</returns>
    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse>> GetCurrentUser()
    {
        var userId = GetCurrentUserId();
        if (userId == null)
        {
            return Unauthorized(ApiResponse.Fail("User not authenticated"));
        }

        var user = await authService.GetUserByIdAsync(userId.Value);
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

    /// <summary>
    /// Logs out the current user
    /// </summary>
    /// <returns>The result of the logout</returns>
    [Authorize]
    [HttpPost("logout")]
    public ActionResult<ApiResponse> Logout()
    {
        Response.Cookies.Delete("access_token");
        return Ok(ApiResponse.Ok(new { message = "Logged out successfully." }));
    }

    /// <summary>
    /// Gets the current user's ID from the claims
    /// </summary>
    /// <returns>The current user's ID or null if not authenticated</returns>
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
