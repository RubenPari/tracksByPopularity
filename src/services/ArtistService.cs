using Newtonsoft.Json;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public class ArtistService : IArtistService
{
    private readonly IHttpClientFactory _httpClientFactory;

    public ArtistService(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

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

    // Legacy static method for backward compatibility
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
