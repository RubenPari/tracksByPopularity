using StackExchange.Redis;
using tracksByPopularity.helpers;
using tracksByPopularity.services;

namespace tracksByPopularity.controllers;

public static class PlaylistController
{
    public static async Task<IResult> CreatePlaylistTrackMinor(
        IConnectionMultiplexer cacheRedisConnection
    )
    {
        var tracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var created = await PlaylistService.CreatePlaylistTracksMinorAsync(tracks);

        return created
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }
}
