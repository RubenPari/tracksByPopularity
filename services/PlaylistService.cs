using System.Net;

namespace tracksByPopularity.services;

public static class PlaylistService
{
    public static async Task<bool> RemoveAllTracks(string playlistId)
    {
        const string baseUrl = "https://clear-songs.fly.dev";
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{baseUrl}/playlist/delete-tracks?playlistId={playlistId}"),
        };
        
        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return response.StatusCode == HttpStatusCode.OK;
    }
}