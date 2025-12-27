namespace tracksByPopularity.services;

/// <summary>
/// Service interface for external authentication checks.
/// Handles authentication verification with external services.
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// Checks if the user is authenticated with the ClearSongs service.
    /// </summary>
    /// <returns>
    /// <c>true</c> if the user is authenticated; otherwise, <c>false</c>.
    /// </returns>
    Task<bool> IsAuthenticatedWithClearSongsServiceAsync();
}

