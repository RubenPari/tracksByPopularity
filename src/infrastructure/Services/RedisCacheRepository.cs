using Newtonsoft.Json;
using StackExchange.Redis;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Redis implementation of ICacheRepository.
/// Follows DIP: Depends on IConnectionMultiplexer abstraction from StackExchange.Redis.
/// </summary>
public class RedisCacheRepository : ICacheRepository
{
    private readonly IConnectionMultiplexer _connection;

    public RedisCacheRepository(IConnectionMultiplexer connection)
    {
        _connection = connection;
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        var db = _connection.GetDatabase();
        var value = await db.StringGetAsync(key);
        
        if (!value.HasValue)
        {
            return null;
        }

        return JsonConvert.DeserializeObject<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var db = _connection.GetDatabase();
        var serialized = JsonConvert.SerializeObject(value);
        await db.StringSetAsync(key, serialized, expiration);
    }

    public async Task RemoveAsync(string key)
    {
        var db = _connection.GetDatabase();
        await db.KeyDeleteAsync(key);
    }

    public async Task<bool> ExistsAsync(string key)
    {
        var db = _connection.GetDatabase();
        return await db.KeyExistsAsync(key);
    }
}
