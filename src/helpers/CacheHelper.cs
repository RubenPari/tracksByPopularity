using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.services;

namespace tracksByPopularity.helpers;

public static class CacheHelper
{
    public static async Task<IList<SavedTrack>> GetAllUserTracksWithClient(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyClient spotifyClient
    )
    {
        var cacheRedis = cacheRedisConnection.GetDatabase();

        var tracksCache = await cacheRedis.StringGetAsync("allTracks");

        IList<SavedTrack> allTracks;

        if (tracksCache.HasValue)
        {
            allTracks = JsonConvert.DeserializeObject<IList<SavedTrack>>(tracksCache!)!;
        }
        else
        {
            allTracks = await TrackService.GetAllUserTracksWithClient(spotifyClient);

            await cacheRedis.StringSetAsync("allTracks", JsonConvert.SerializeObject(allTracks));
        }

        return allTracks;
    }
}
