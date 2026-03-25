using Microsoft.Extensions.Options;
using tracksByPopularity.Infrastructure.Configuration;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Service implementation for external authentication checks.
/// Handles authentication verification with the ClearSongs service.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthenticationService> _logger;
    private readonly string _clearSongsAuthUrl;

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    public AuthenticationService(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthenticationService> logger,
        IOptions<AppSettings> appSettings
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _clearSongsAuthUrl = $"{appSettings.Value.ClearSongsBaseUrl}/auth/is-auth";
    }

    /// <summary>
    /// Checks if the user is authenticated with the ClearSongs service.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
    /// </returns>
    /// <remarks>
    /// This method calls an external service endpoint to verify authentication status.
    /// If the HTTP request fails or returns a non-success status code, the method returns false.
    /// </remarks>
    public async Task<bool> IsAuthenticatedWithClearSongsServiceAsync()
    {
        try
        {
            var http = _httpClientFactory.CreateClient();
            var response = await http.GetAsync(_clearSongsAuthUrl);
            
            var isAuthenticated = response.IsSuccessStatusCode;
            
            if (!isAuthenticated)
            {
                _logger.LogWarning("Authentication check failed with status code: {StatusCode}", response.StatusCode);
            }
            
            return isAuthenticated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking authentication with ClearSongs service");
            return false;
        }
    }
}

