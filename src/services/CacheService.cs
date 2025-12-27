using Newtonsoft.Json;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.services;

namespace tracksByPopularity.services;

public class CacheService : ICacheService
{
    private readonly IConnectionMultiplexer _cacheRedisConnection;
    private readonly ITrackService _trackService;

    public CacheService(
        IConnectionMultiplexer cacheRedisConnection,
        ITrackService trackService
    )
    {
        _cacheRedisConnection = cacheRedisConnection;
        _trackService = trackService;
    }

    public async Task<IList<SavedTrack>> GetAllUserTracksWithClientAsync(
        SpotifyClient spotifyClient
    )
    {
        var cacheRedis = _cacheRedisConnection.GetDatabase();

        var tracksCache = await cacheRedis.StringGetAsync("allTracks");

        IList<SavedTrack> allTracks;

        if (tracksCache.HasValue)
        {
            allTracks = JsonConvert.DeserializeObject<IList<SavedTrack>>(tracksCache!)!;
        }
        else
        {
            allTracks = await _trackService.GetAllUserTracksWithClientAsync(spotifyClient);

            await cacheRedis.StringSetAsync(
                "allTracks",
                JsonConvert.SerializeObject(allTracks)
            );
        }

        return allTracks;
    }
}

