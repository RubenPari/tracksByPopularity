using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.services;

namespace tracksByPopularity.helpers;

public static class CacheHelper
{
    public static async Task<IList<SavedTrack>> GetAllUserTracks(
        IConnectionMultiplexer cacheRedisConnection,
        SpotifyClient spotifyClient
    )
    {
        var cacheRedis = cacheRedisConnection.GetDatabase();

        // Get user ID for cache key to ensure user-specific caching
        var userId = (await spotifyClient.UserProfile.Current()).Id;
        var cacheKey = $"tracks:{userId}";

        var tracksCache = await cacheRedis.StringGetAsync(cacheKey);

        IList<SavedTrack> allTracks;

        if (tracksCache.HasValue)
        {
            allTracks = JsonConvert.DeserializeObject<IList<SavedTrack>>(tracksCache!)!;
        }
        else
        {
            allTracks = await TrackService.GetAllUserTracks(spotifyClient);

            // Set expiration time to 1 hour for cached tracks
            await cacheRedis.StringSetAsync(
                cacheKey,
                JsonConvert.SerializeObject(allTracks),
                TimeSpan.FromHours(1)
            );
        }

        return allTracks;
    }
}
