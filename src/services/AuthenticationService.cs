namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for external authentication checks.
/// Handles authentication verification with the ClearSongs service.
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<AuthenticationService> _logger;
    private const string ClearSongsAuthUrl = "http://localhost:3000/auth/is-auth";

    /// <summary>
    /// Initializes a new instance of the <see cref="AuthenticationService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients.</param>
    /// <param name="logger">Logger instance for recording authentication attempts.</param>
    public AuthenticationService(
        IHttpClientFactory httpClientFactory,
        ILogger<AuthenticationService> logger
    )
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
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
            var response = await http.GetAsync(ClearSongsAuthUrl);
            
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

