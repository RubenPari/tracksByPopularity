using System.Net;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public static class PlaylistService
{
    public static async Task<RemoveAllTracksResponse> RemoveAllTracks(string playlistId)
    {
        const string baseUrl = "https://clear-songs.fly.dev";
        var client = new HttpClient();
        var request = new HttpRequestMessage
        {
            Method = HttpMethod.Get,
            RequestUri = new Uri($"{baseUrl}/playlist/delete-tracks?id_playlist={playlistId}"),
        };

        using var response = await client.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => RemoveAllTracksResponse.Unauthorized,
            HttpStatusCode.BadRequest => RemoveAllTracksResponse.BadRequest,
            _ => RemoveAllTracksResponse.Success
        };
    }
}