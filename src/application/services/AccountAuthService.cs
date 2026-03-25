using Microsoft.EntityFrameworkCore;
using tracksByPopularity.Domain.Entities;
using tracksByPopularity.Infrastructure.Data;
using tracksByPopularity.Infrastructure.Services;
using BCrypt.Net;

namespace tracksByPopularity.Application.Services;

public interface IAccountAuthService
{
    Task<AuthResult> RegisterAsync(string email, string password);
    Task<LoginResult> LoginAsync(string email, string password);
    Task<VerifyResult> VerifyEmailAsync(string token);
    Task<ForgotPasswordResult> ForgotPasswordAsync(string email);
    Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword);
    Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword);
    Task<LinkSpotifyResult> LinkSpotifyAsync(Guid userId, string spotifyUserId, string accessToken, string refreshToken, DateTime expiresAt);
    Task<User?> GetUserByIdAsync(Guid userId);
    Task<User?> GetUserByEmailAsync(string email);
    Task<SpotifyLink?> GetSpotifyLinkAsync(Guid userId);
    Task<SpotifyLink?> GetSpotifyLinkBySpotifyIdAsync(string spotifyUserId);
    Task<User?> GetOrCreateUserFromSpotifyAsync(string spotifyUserId, string accessToken, string refreshToken, DateTime expiresAt);
    Task UnlinkSpotifyAsync(Guid userId);
    Task<SpotifyLink?> RefreshSpotifyTokenAsync(string spotifyUserId);
}

public class AuthResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Token { get; init; }
    public User? User { get; init; }
}

public class LoginResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Token { get; init; }
    public User? User { get; init; }
    public bool EmailNotVerified { get; init; }
}

public class VerifyResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class ForgotPasswordResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class ResetPasswordResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class ChangePasswordResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
}

public class LinkSpotifyResult
{
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? Token { get; init; }
}

public class AccountAuthService : IAccountAuthService
{
    private readonly AppDbContext _dbContext;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;
    private readonly ILogger<AccountAuthService> _logger;

    public AccountAuthService(
        AppDbContext dbContext,
        IJwtService jwtService,
        IEmailService emailService,
        ILogger<AccountAuthService> logger)
    {
        _dbContext = dbContext;
        _jwtService = jwtService;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();

        if (await _dbContext.Users.AnyAsync(u => u.Email.ToLower() == normalizedEmail))
        {
            return new AuthResult { Success = false, Error = "Email already registered" };
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsEmailVerified = false
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var verificationToken = GenerateVerificationToken(user.Id);
        await _emailService.SendVerificationEmailAsync(user.Email, verificationToken);

        _logger.LogInformation("User registered: {Email}", email);

        return new AuthResult { Success = true, User = user };
    }

    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            return new LoginResult { Success = false, Error = "Invalid email or password" };
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new LoginResult { Success = false, Error = "Invalid email or password" };
        }

        var token = _jwtService.GenerateToken(user.Id, user.Email);

        _logger.LogInformation("User logged in: {Email}", email);

        return new LoginResult { Success = true, Token = token, User = user };
    }

    public async Task<VerifyResult> VerifyEmailAsync(string token)
    {
        var verificationToken = await _dbContext.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

        if (verificationToken == null)
        {
            return new VerifyResult { Success = false, Error = "Invalid or expired token" };
        }

        if (verificationToken.ExpiresAt < DateTime.UtcNow)
        {
            return new VerifyResult { Success = false, Error = "Token has expired" };
        }

        verificationToken.IsUsed = true;
        verificationToken.User.IsEmailVerified = true;
        verificationToken.User.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Email verified for user: {Email}", verificationToken.User.Email);

        return new VerifyResult { Success = true };
    }

    public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email)
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());

        if (user == null)
        {
            return new ForgotPasswordResult { Success = true };
        }

        var resetToken = GenerateResetToken(user.Id);
        await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

        _logger.LogInformation("Password reset requested for: {Email}", email);

        return new ForgotPasswordResult { Success = true };
    }

    public async Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword)
    {
        var resetToken = await _dbContext.PasswordResetTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && !t.IsUsed);

        if (resetToken == null)
        {
            return new ResetPasswordResult { Success = false, Error = "Invalid or expired token" };
        }

        if (resetToken.ExpiresAt < DateTime.UtcNow)
        {
            return new ResetPasswordResult { Success = false, Error = "Token has expired" };
        }

        resetToken.IsUsed = true;
        resetToken.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        resetToken.User.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Password reset completed for user: {Email}", resetToken.User.Email);

        return new ResetPasswordResult { Success = true };
    }

    public async Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            return new ChangePasswordResult { Success = false, Error = "User not found" };
        }

        if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
        {
            return new ChangePasswordResult { Success = false, Error = "Current password is incorrect" };
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Password changed for user: {Email}", user.Email);

        return new ChangePasswordResult { Success = true };
    }

    public async Task<LinkSpotifyResult> LinkSpotifyAsync(Guid userId, string spotifyUserId, string accessToken, string refreshToken, DateTime expiresAt)
    {
        var user = await _dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            return new LinkSpotifyResult { Success = false, Error = "User not found" };
        }

        var existingLink = await _dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);

        if (existingLink != null && existingLink.UserId != userId)
        {
            return new LinkSpotifyResult { Success = false, Error = "This Spotify account is already linked to another user" };
        }

        if (existingLink != null)
        {
            existingLink.AccessToken = accessToken;
            existingLink.RefreshToken = refreshToken;
            existingLink.TokenExpiresAt = expiresAt;
            existingLink.UpdatedAt = DateTime.UtcNow;
        }
        else
        {
            var spotifyLink = new SpotifyLink
            {
                UserId = userId,
                SpotifyUserId = spotifyUserId,
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                TokenExpiresAt = expiresAt
            };

            _dbContext.SpotifyLinks.Add(spotifyLink);
        }

        await _dbContext.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user.Id, user.Email);

        _logger.LogInformation("Spotify linked for user: {Email}", user.Email);

        return new LinkSpotifyResult { Success = true, Token = token };
    }

    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await _dbContext.Users
            .Include(u => u.SpotifyLink)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await _dbContext.Users
            .Include(u => u.SpotifyLink)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == email.ToLower());
    }

    public async Task<SpotifyLink?> GetSpotifyLinkAsync(Guid userId)
    {
        return await _dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.UserId == userId);
    }

    public async Task<SpotifyLink?> GetSpotifyLinkBySpotifyIdAsync(string spotifyUserId)
    {
        return await _dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);
    }

    public async Task<User?> GetOrCreateUserFromSpotifyAsync(string spotifyUserId, string accessToken, string refreshToken, DateTime expiresAt)
    {
        var existingLink = await _dbContext.SpotifyLinks
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);

        if (existingLink != null)
        {
            existingLink.AccessToken = accessToken;
            existingLink.RefreshToken = refreshToken;
            existingLink.TokenExpiresAt = expiresAt;
            existingLink.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            return existingLink.User;
        }

        var spotifyUserEmail = $"{spotifyUserId}@spotify.placeholder";
        var user = new User
        {
            Email = spotifyUserEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            IsEmailVerified = true
        };

        _dbContext.Users.Add(user);
        await _dbContext.SaveChangesAsync();

        var spotifyLink = new SpotifyLink
        {
            UserId = user.Id,
            SpotifyUserId = spotifyUserId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = expiresAt
        };

        _dbContext.SpotifyLinks.Add(spotifyLink);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Created new user from Spotify: {SpotifyUserId}", spotifyUserId);

        return user;
    }

    public async Task UnlinkSpotifyAsync(Guid userId)
    {
        var spotifyLink = await _dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.UserId == userId);

        if (spotifyLink != null)
        {
            _dbContext.SpotifyLinks.Remove(spotifyLink);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Spotify unlinked for user: {UserId}", userId);
        }
    }

    public async Task<SpotifyLink?> RefreshSpotifyTokenAsync(string spotifyUserId)
    {
        var spotifyLink = await _dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);
        return spotifyLink;
    }

    private string GenerateVerificationToken(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-");
        var verificationToken = new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        _dbContext.EmailVerificationTokens.Add(verificationToken);
        _dbContext.SaveChanges();

        return token;
    }

    private string GenerateResetToken(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-");
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        _dbContext.PasswordResetTokens.Add(resetToken);
        _dbContext.SaveChanges();

        return token;
    }
}
