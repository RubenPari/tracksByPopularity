using Newtonsoft.Json;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for artist-related operations.
/// Handles retrieval of artist summary information from external services.
/// </summary>
public class ArtistService : IArtistService
{
    private readonly IHttpClientFactory _httpClientFactory;

    /// <summary>
    /// Initializes a new instance of the <see cref="ArtistService"/> class.
    /// </summary>
    /// <param name="httpClientFactory">Factory for creating HTTP clients for external service calls.</param>
    public ArtistService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Retrieves a summary of all artists in the user's library from an external service.
    /// The summary includes artist ID, name, and the count of tracks for each artist.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="ArtistSummary"/> objects, or <c>null</c> if the request fails.
    /// </returns>
    /// <remarks>
    /// This method calls the external service at http://localhost:3030/track/summary.
    /// If the HTTP request is not successful, the method returns null.
    /// </remarks>
    public async Task<IEnumerable<ArtistSummary>?> GetArtistsSummaryAsync()
    {
        var http = _httpClientFactory.CreateClient();

        var response = await http.GetAsync("http://localhost:3030/track/summary");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var jsonResult = await response.Content.ReadAsStringAsync();

        var artists = JsonConvert.DeserializeObject<ArtistSummary[]>(jsonResult)!;

        return artists;
    }

    /// <summary>
    /// Legacy static method for backward compatibility.
    /// </summary>
    /// <returns>
    /// An enumerable of <see cref="ArtistSummary"/> objects, or <c>null</c> if the request fails.
    /// </returns>
    /// <remarks>
    /// This method is deprecated. Use <see cref="IArtistService.GetArtistsSummaryAsync"/> instead
    /// through dependency injection for better testability and resource management.
    /// </remarks>
    [Obsolete("Use IArtistService.GetArtistsSummaryAsync instead")]
    public static async Task<IEnumerable<ArtistSummary>?> GetArtistsSummary()
    {
        var http = new HttpClient();
        var response = await http.GetAsync("http://localhost:3030/track/summary");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var jsonResult = await response.Content.ReadAsStringAsync();
        var artists = JsonConvert.DeserializeObject<ArtistSummary[]>(jsonResult)!;

        return artists;
    }
}
