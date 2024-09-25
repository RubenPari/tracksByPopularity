using System.Net;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public static class PlaylistService
{
    public static async Task<RemoveAllTracksResponse> RemoveAllTracks(string playlistId)
    {
        var client = new HttpClient();
        client.Timeout = TimeSpan.FromSeconds(200);
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Delete,
            RequestUri = new Uri($"http://localhost:3000/playlist/delete-tracks?id={playlistId}"),
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => RemoveAllTracksResponse.Unauthorized,
            HttpStatusCode.BadRequest => RemoveAllTracksResponse.BadRequest,
            _ => RemoveAllTracksResponse.Success,
        };
    }
}
