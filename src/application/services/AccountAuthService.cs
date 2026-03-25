using Microsoft.EntityFrameworkCore;
using tracksByPopularity.Infrastructure.Data;

namespace tracksByPopularity.Application.Services;

public class AccountAuthService(
    AppDbContext dbContext,
    IJwtService jwtService,
    IEmailService emailService,
    ILogger<AccountAuthService> logger)
    : IAccountAuthService
{
    /// <summary>
    /// Registers a new user with the provided email and password.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>An <see cref="AuthResult"/> indicating the success or failure of the registration.</returns>
    public async Task<AuthResult> RegisterAsync(string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();

        if (await dbContext.Users.AnyAsync(u => u.Email.Equals(normalizedEmail, StringComparison.CurrentCultureIgnoreCase)))
        {
            return new AuthResult { Success = false, Error = "Email already registered" };
        }

        var user = new User
        {
            Email = normalizedEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
            IsEmailVerified = false
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var verificationToken = GenerateVerificationToken(user.Id);
        await emailService.SendVerificationEmailAsync(user.Email, verificationToken);

        logger.LogInformation("User registered: {Email}", email);

        return new AuthResult { Success = true, User = user };
    }

    /// <summary>
    /// Logs in a user with the provided email and password.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <param name="password">The password of the user.</param>
    /// <returns>A <see cref="LoginResult"/> indicating the success or failure of the login.</returns>
    public async Task<LoginResult> LoginAsync(string email, string password)
    {
        var normalizedEmail = email.ToLowerInvariant();
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(normalizedEmail, StringComparison.CurrentCultureIgnoreCase));

        if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            return new LoginResult { Success = false, Error = "Invalid email or password" };
        }

        var token = jwtService.GenerateToken(user.Id, user.Email);

        logger.LogInformation("User logged in: {Email}", email);

        return new LoginResult { Success = true, Token = token, User = user };
    }

    /// <summary>
    /// Verifies a user's email address using a verification token.
    /// </summary>
    /// <param name="token">The verification token.</param>
    /// <returns>A <see cref="VerifyResult"/> indicating the success or failure of the verification.</returns>
    public async Task<VerifyResult> VerifyEmailAsync(string token)
    {
        var verificationToken = await dbContext.EmailVerificationTokens
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

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Email verified for user: {Email}", verificationToken.User.Email);

        return new VerifyResult { Success = true };
    }

    /// <summary>
    /// Initiates the password reset process for a user.
    /// </summary>
    /// <param name="email">The email address of the user.</param>
    /// <returns>A <see cref="ForgotPasswordResult"/> indicating the success or failure of the request.</returns>
    public async Task<ForgotPasswordResult> ForgotPasswordAsync(string email)
    {
        var user = await dbContext.Users.FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));

        if (user == null)
        {
            return new ForgotPasswordResult { Success = true };
        }

        var resetToken = GenerateResetToken(user.Id);
        await emailService.SendPasswordResetEmailAsync(user.Email, resetToken);

        logger.LogInformation("Password reset requested for: {Email}", email);

        return new ForgotPasswordResult { Success = true };
    }

    /// <summary>
    /// Resets a user's password using a reset token.
    /// </summary>
    /// <param name="token">The reset token.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>A <see cref="ResetPasswordResult"/> indicating the success or failure of the reset.</returns>
    public async Task<ResetPasswordResult> ResetPasswordAsync(string token, string newPassword)
    {
        var resetToken = await dbContext.PasswordResetTokens
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

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Password reset completed for user: {Email}", resetToken.User.Email);

        return new ResetPasswordResult { Success = true };
    }

    /// <summary>
    /// Changes a user's password.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="oldPassword">The current password.</param>
    /// <param name="newPassword">The new password.</param>
    /// <returns>A <see cref="ChangePasswordResult"/> indicating the success or failure of the password change.</returns>
    public async Task<ChangePasswordResult> ChangePasswordAsync(Guid userId, string oldPassword, string newPassword)
    {
        var user = await dbContext.Users.FindAsync(userId);

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

        await dbContext.SaveChangesAsync();

        logger.LogInformation("Password changed for user: {Email}", user.Email);

        return new ChangePasswordResult { Success = true };
    }

    /// <summary>
    /// Links a Spotify account to a user.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <param name="spotifyUserId">The Spotify user ID.</param>
    /// <param name="accessToken">The Spotify access token.</param>
    /// <param name="refreshToken">The Spotify refresh token.</param>
    /// <param name="expiresAt">The expiration date of the access token.</param>
    /// <returns>A <see cref="LinkSpotifyResult"/> indicating the success or failure of the linking.</returns>
    public async Task<LinkSpotifyResult> LinkSpotifyAsync(Guid userId, string spotifyUserId, string accessToken, string refreshToken, DateTime expiresAt)
    {
        var user = await dbContext.Users.FindAsync(userId);

        if (user == null)
        {
            return new LinkSpotifyResult { Success = false, Error = "User not found" };
        }

        var existingLink = await dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);

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

            dbContext.SpotifyLinks.Add(spotifyLink);
        }

        await dbContext.SaveChangesAsync();

        var token = jwtService.GenerateToken(user.Id, user.Email);

        logger.LogInformation("Spotify linked for user: {Email}", user.Email);

        return new LinkSpotifyResult { Success = true, Token = token };
    }

    /// <summary>
    /// Gets a user by ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    public async Task<User?> GetUserByIdAsync(Guid userId)
    {
        return await dbContext.Users
            .Include(u => u.SpotifyLink)
            .FirstOrDefaultAsync(u => u.Id == userId);
    }

    /// <summary>
    /// Gets a user by email.
    /// </summary>
    /// <param name="email">The email of the user.</param>
    /// <returns>The user if found; otherwise, null.</returns>
    public async Task<User?> GetUserByEmailAsync(string email)
    {
        return await dbContext.Users
            .Include(u => u.SpotifyLink)
            .FirstOrDefaultAsync(u => u.Email.Equals(email, StringComparison.CurrentCultureIgnoreCase));
    }

    /// <summary>
    /// Gets a Spotify link by user ID.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>The Spotify link if found; otherwise, null.</returns>
    public async Task<SpotifyLink?> GetSpotifyLinkAsync(Guid userId)
    {
        return await dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.UserId == userId);
    }

    /// <summary>
    /// Gets a Spotify link by Spotify user ID.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID.</param>
    /// <returns>The Spotify link if found; otherwise, null.</returns>
    public async Task<SpotifyLink?> GetSpotifyLinkBySpotifyIdAsync(string spotifyUserId)
    {
        return await dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);
    }

    /// <summary>
    /// Gets or creates a user from Spotify information.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID.</param>
    /// <param name="accessToken">The Spotify access token.</param>
    /// <param name="refreshToken">The Spotify refresh token.</param>
    /// <param name="expiresAt">The expiration date of the access token.</param>
    /// <returns>The user associated with the Spotify account.</returns>
    public async Task<User?> GetOrCreateUserFromSpotifyAsync(string spotifyUserId, string accessToken, string refreshToken, DateTime expiresAt)
    {
        var existingLink = await dbContext.SpotifyLinks
            .Include(s => s.User)
            .FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);

        if (existingLink != null)
        {
            existingLink.AccessToken = accessToken;
            existingLink.RefreshToken = refreshToken;
            existingLink.TokenExpiresAt = expiresAt;
            existingLink.UpdatedAt = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();
            return existingLink.User;
        }

        var spotifyUserEmail = $"{spotifyUserId}@spotify.placeholder";
        var user = new User
        {
            Email = spotifyUserEmail,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()),
            IsEmailVerified = true
        };

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();

        var spotifyLink = new SpotifyLink
        {
            UserId = user.Id,
            SpotifyUserId = spotifyUserId,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            TokenExpiresAt = expiresAt
        };

        dbContext.SpotifyLinks.Add(spotifyLink);
        await dbContext.SaveChangesAsync();

        logger.LogInformation("Created new user from Spotify: {SpotifyUserId}", spotifyUserId);

        return user;
    }

    /// <summary>
    /// Unlinks the Spotify account for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to unlink Spotify for.</param>
    public async Task UnlinkSpotifyAsync(Guid userId)
    {
        var spotifyLink = await dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.UserId == userId);

        if (spotifyLink != null)
        {
            dbContext.SpotifyLinks.Remove(spotifyLink);
            await dbContext.SaveChangesAsync();

            logger.LogInformation("Spotify unlinked for user: {UserId}", userId);
        }
    }

    /// <summary>
    /// Refreshes the Spotify token for the specified user.
    /// </summary>
    /// <param name="spotifyUserId">The Spotify user ID to refresh the token for.</param>
    /// <returns>The refreshed Spotify link, or null if the user was not found.</returns>
    public async Task<SpotifyLink?> RefreshSpotifyTokenAsync(string spotifyUserId)
    {
        var spotifyLink = await dbContext.SpotifyLinks.FirstOrDefaultAsync(s => s.SpotifyUserId == spotifyUserId);
        return spotifyLink;
    }

    /// <summary>
    /// Generates a verification token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to generate a token for.</param>
    /// <returns>The generated verification token.</returns>
    private string GenerateVerificationToken(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-");
        var verificationToken = new EmailVerificationToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(24)
        };

        dbContext.EmailVerificationTokens.Add(verificationToken);
        dbContext.SaveChanges();

        return token;
    }

    /// <summary>
    /// Generates a password reset token for the specified user.
    /// </summary>
    /// <param name="userId">The ID of the user to generate a token for.</param>
    /// <returns>The generated password reset token.</returns>
    private string GenerateResetToken(Guid userId)
    {
        var token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()).Replace("/", "_").Replace("+", "-");
        var resetToken = new PasswordResetToken
        {
            UserId = userId,
            Token = token,
            ExpiresAt = DateTime.UtcNow.AddHours(1)
        };

        dbContext.PasswordResetTokens.Add(resetToken);
        dbContext.SaveChanges();

        return token;
    }
}
