using Newtonsoft.Json;
using tracksByPopularity.models;
using tracksByPopularity.utils;

namespace tracksByPopularity.services;

public abstract class ArtistService
{
    public static async Task<IEnumerable<ArtistSummary>?> GetArtistsSummary()
    {
        var http = new HttpClient();

        var response = await http.GetAsync($"{Constants.ClearSongsBaseUrl}/track/summary");

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var jsonResult = await response.Content.ReadAsStringAsync();

        var artists = JsonConvert.DeserializeObject<ArtistSummary[]>(jsonResult)!;

        return artists;
    }
}
