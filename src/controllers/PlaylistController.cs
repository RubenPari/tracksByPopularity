using StackExchange.Redis;
using tracksByPopularity.helpers;
using tracksByPopularity.services;

namespace tracksByPopularity.controllers;

public static class PlaylistController
{
    public static async Task<IResult> CreatePlaylistTrackMinor(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyAuthService spotifyAuthService
    )
    {
        try
        {
            var spotifyClient = SpotifyAuthService.GetSpotifyClientAsync();

            var tracks = await CacheHelper.GetAllUserTracksWithClient(
                cacheRedisConnection,
                spotifyClient
            );

            var created = await PlaylistService.CreatePlaylistTracksMinorAsync(
                spotifyClient,
                tracks
            );

            return created
                ? Results.Ok("Tracks added to playlist")
                : Results.BadRequest("Failed to add tracks to playlist");
        }
        catch (UnauthorizedAccessException)
        {
            return Results.Unauthorized();
        }
        catch (Exception ex)
        {
            return Results.Problem($"Error: {ex.Message}");
        }
    }
}
