using Newtonsoft.Json;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public abstract class ArtistService
{
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
