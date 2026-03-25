using System.ComponentModel.DataAnnotations;

namespace tracksByPopularity.Application.DTOs;

/// <summary>
/// Represents a request to register a new user.
/// - Email: The email address of the user.
/// - Password: The password of the user.
/// </summary>
public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Represents a request to login a user.
/// - Email: The email address of the user.
/// - Password: The password of the user.
/// </summary>
public class AccountLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

/// <summary>
/// Represents a request to send a password reset email.
/// - Email: The email address of the user.
/// </summary>
public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}

/// <summary>
/// Represents a request to reset a user's password.
/// - Token: The token sent to the user's email.
/// - NewPassword: The new password of the user.
/// </summary>
public class ResetPasswordRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;
}

/// <summary>
/// Represents a request to change a user's password.
/// - OldPassword: The old password of the user.
/// - NewPassword: The new password of the user.
/// </summary>
public class ChangePasswordRequest
{
    [Required]
    public string OldPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;
}

/// <summary>
/// Represents a request to link a user's Spotify account.
/// - SpotifyUserId: The Spotify user ID.
/// - AccessToken: The Spotify access token.
/// - RefreshToken: The Spotify refresh token.
/// - TokenExpiresAt: The expiration date of the Spotify access token.
/// </summary>
public class LinkSpotifyRequest
{
    [Required]
    public string SpotifyUserId { get; init; } = string.Empty;

    [Required]
    public string AccessToken { get; init; } = string.Empty;

    [Required]
    public string RefreshToken { get; init; } = string.Empty;

    public DateTime TokenExpiresAt { get; init; }
}

/// <summary>
/// Represents a user DTO.
/// - Id: The ID of the user.
/// - Email: The email address of the user.
/// - IsEmailVerified: Whether the user's email is verified.
/// - IsSpotifyLinked: Whether the user's Spotify account is linked.
/// </summary>
public class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool IsEmailVerified { get; init; }
    public bool IsSpotifyLinked { get; init; }
}

/// <summary>
/// Represents a login response.
/// - Token: The JWT token.
/// - User: The user DTO.
/// </summary>
public class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = null!;
}

/// <summary>
/// Represents a Spotify link status DTO.
/// - IsLinked: Whether the user's Spotify account is linked.
/// - SpotifyUserId: The Spotify user ID.
/// </summary>
public class SpotifyLinkStatusDto
{
    public bool IsLinked { get; init; }
    public string? SpotifyUserId { get; init; }
}
