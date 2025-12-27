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
            // Use the instance method through a temporary service instance
            // Note: This is a legacy helper. Consider using ICacheService instead.
            var trackService = new TrackService();
            allTracks = await trackService.GetAllUserTracksWithClientAsync(spotifyClient);

            await cacheRedis.StringSetAsync("allTracks", JsonConvert.SerializeObject(allTracks));
        }

        return allTracks;
    }
}
