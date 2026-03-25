namespace tracksByPopularity.Application.Interfaces;

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
    Task<SpotifyLink?> GetSpotifyLinkAsync(Guid userId);
    Task UnlinkSpotifyAsync(Guid userId);
}