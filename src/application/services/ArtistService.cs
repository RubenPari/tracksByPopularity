using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Service implementation for artist-related operations.
/// Handles retrieval of artist summary information from external services.
/// </summary>
public class ArtistService(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
    : IArtistService
{
    private readonly string _trackSummaryUrl = $"{appSettings.Value.TrackSummaryBaseUrl}/track/summary";

    /// <summary>
    /// Retrieves a summary of all artists in the user's library from an external service.
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation. The task result contains an enumerable of ArtistSummary objects, or null if the operation fails.
    /// </returns>
    public async Task<IEnumerable<ArtistSummary>?> GetArtistsSummaryAsync()
    {
        var http = httpClientFactory.CreateClient();

        var response = await http.GetAsync(_trackSummaryUrl);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var jsonResult = await response.Content.ReadAsStringAsync();

        var artists = JsonConvert.DeserializeObject<ArtistSummary[]>(jsonResult)!;

        return artists;
    }
}
