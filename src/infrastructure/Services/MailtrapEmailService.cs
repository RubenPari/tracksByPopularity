using System.Text;
using System.Text.Json;
using tracksByPopularity.Infrastructure.Configuration;

namespace tracksByPopularity.Infrastructure.Services;

public interface IEmailService
{
    Task SendEmailAsync(string to, string subject, string htmlContent);
    Task SendVerificationEmailAsync(string to, string token);
    Task SendPasswordResetEmailAsync(string to, string token);
}

public class MailtrapEmailService : IEmailService
{
    private readonly MailtrapSettings _settings;
    private readonly HttpClient _httpClient;
    private readonly ILogger<MailtrapEmailService> _logger;

    public MailtrapEmailService(
        MailtrapSettings settings,
        IHttpClientFactory httpClientFactory,
        ILogger<MailtrapEmailService> logger)
    {
        _settings = settings;
        _httpClient = httpClientFactory.CreateClient("Mailtrap");
        _httpClient.DefaultRequestHeaders.Add("Api-Key", _settings.ApiKey);
        _logger = logger;
    }

    public async Task SendEmailAsync(string to, string subject, string htmlContent)
    {
        if (string.IsNullOrEmpty(_settings.ApiKey))
        {
            _logger.LogWarning("Mailtrap API key not configured. Email not sent to {To}", to);
            return;
        }

        var payload = new
        {
            from = new { email = _settings.FromEmail, name = _settings.FromName },
            to = new[] { new { email = to } },
            subject,
            html = htmlContent
        };

        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8,
            "application/json"
        );

        try
        {
            var response = await _httpClient.PostAsync(
                "https://send.api.mailtrap.io/api/send",
                content
            );

            if (!response.IsSuccessStatusCode)
            {
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogError("Failed to send email: {StatusCode} - {Response}", response.StatusCode, responseBody);
            }
            else
            {
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending email to {To}", to);
        }
    }

    public async Task SendVerificationEmailAsync(string to, string token)
    {
        var verificationUrl = $"{_settings.ClientUrl}/verify-email/{token}";

        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background: linear-gradient(135deg, #6366f1, #8b5cf6); color: white; text-decoration: none; border-radius: 8px; font-weight: bold; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Verify Your Email Address</h2>
        <p>Thank you for registering with Tracks by Popularity!</p>
        <p>Please click the button below to verify your email address:</p>
        <p><a href='{verificationUrl}' class='button'>Verify Email</a></p>
        <p>Or copy and paste this link into your browser:</p>
        <p><a href='{verificationUrl}'>{verificationUrl}</a></p>
        <p>This link expires in 24 hours.</p>
        <div class='footer'>
            <p>If you didn't create an account, please ignore this email.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, "Verify Your Email - Tracks by Popularity", htmlContent);
    }

    public async Task SendPasswordResetEmailAsync(string to, string token)
    {
        var resetUrl = $"{_settings.ClientUrl}/reset-password/{token}";

        var htmlContent = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <style>
        body {{ font-family: Arial, sans-serif; line-height: 1.6; color: #333; }}
        .container {{ max-width: 600px; margin: 0 auto; padding: 20px; }}
        .button {{ display: inline-block; padding: 12px 24px; background: linear-gradient(135deg, #ef4444, #f97316); color: white; text-decoration: none; border-radius: 8px; font-weight: bold; }}
        .footer {{ margin-top: 20px; font-size: 12px; color: #666; }}
    </style>
</head>
<body>
    <div class='container'>
        <h2>Reset Your Password</h2>
        <p>We received a request to reset your password.</p>
        <p>Please click the button below to reset your password:</p>
        <p><a href='{resetUrl}' class='button'>Reset Password</a></p>
        <p>Or copy and paste this link into your browser:</p>
        <p><a href='{resetUrl}'>{resetUrl}</a></p>
        <p>This link expires in 1 hour.</p>
        <div class='footer'>
            <p>If you didn't request a password reset, please ignore this email and your password will remain unchanged.</p>
        </div>
    </div>
</body>
</html>";

        await SendEmailAsync(to, "Reset Your Password - Tracks by Popularity", htmlContent);
    }
}
