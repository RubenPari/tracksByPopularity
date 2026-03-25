using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using tracksByPopularity.Domain.Enums;
using tracksByPopularity.Infrastructure.Configuration;

namespace tracksByPopularity.Application.Services;

/// <summary>
/// Service implementation for artist-related operations.
/// Handles retrieval of artist summary information from external services.
/// </summary>
public class ArtistService : IArtistService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly string _trackSummaryUrl;

    public ArtistService(IHttpClientFactory httpClientFactory, IOptions<AppSettings> appSettings)
    {
        _httpClientFactory = httpClientFactory;
        _trackSummaryUrl = $"{appSettings.Value.TrackSummaryBaseUrl}/track/summary";
    }

    /// <summary>
    /// Retrieves a summary of all artists in the user's library from an external service.
    /// </summary>
    public async Task<IEnumerable<ArtistSummary>?> GetArtistsSummaryAsync()
    {
        var http = _httpClientFactory.CreateClient();

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
