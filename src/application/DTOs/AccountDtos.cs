using System.ComponentModel.DataAnnotations;

namespace tracksByPopularity.Application.DTOs;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string Password { get; init; } = string.Empty;
}

public class AccountLoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;

    [Required]
    public string Password { get; init; } = string.Empty;
}

public class ForgotPasswordRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; init; } = string.Empty;
}

public class ResetPasswordRequest
{
    [Required]
    public string Token { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;
}

public class ChangePasswordRequest
{
    [Required]
    public string OldPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    public string NewPassword { get; init; } = string.Empty;
}

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

public class UserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public bool IsEmailVerified { get; init; }
    public bool IsSpotifyLinked { get; init; }
}

public class LoginResponse
{
    public string Token { get; init; } = string.Empty;
    public UserDto User { get; init; } = null!;
}

public class SpotifyLinkStatusDto
{
    public bool IsLinked { get; init; }
    public string? SpotifyUserId { get; init; }
}
