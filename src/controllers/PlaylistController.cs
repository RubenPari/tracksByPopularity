using StackExchange.Redis;
using tracksByPopularity.helpers;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.controllers;

public static class PlaylistController
{
    public static async Task<IResult> CreatePlaylistTrackMinor(
        IConnectionMultiplexer cacheRedisConnection,
        ISpotifyClientAccessor spotifyClientAccessor
    )
    {
        try
        {
            var spotifyClient = spotifyClientAccessor.GetClient();
            var tracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection, spotifyClient);

            var created = await PlaylistService.CreatePlaylistTracksMinorAsync(
                tracks,
                spotifyClient
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
            return Results.BadRequest($"Error: {ex.Message}");
        }
    }
}
