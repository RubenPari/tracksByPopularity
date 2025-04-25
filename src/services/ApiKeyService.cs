using Newtonsoft.Json;
using StackExchange.Redis;
using tracksByPopularity.models;

namespace tracksByPopularity.services;

public class ApiKeyService(IConnectionMultiplexer redis)
{
    public async Task<ApiKey> GenerateApiKeyAsync(
        string userId,
        string description,
        int expiryDays = 30
    )
    {
        var apiKey = new ApiKey
        {
            Key = Guid.NewGuid().ToString("N"),
            UserId = userId,
            Description = description,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.AddDays(expiryDays),
            IsActive = true
        };

        var db = redis.GetDatabase();

        // Store API key with its metadata
        await db.StringSetAsync(
            $"api_key:{apiKey.Key}",
            JsonConvert.SerializeObject(apiKey),
            TimeSpan.FromDays(expiryDays)
        );

        // Store a reference to the API key for this user
        await db.SetAddAsync($"user_api_keys:{userId}", apiKey.Key);

        return apiKey;
    }

    public async Task<ApiKey?> ValidateApiKeyAsync(string key)
    {
        var db = redis.GetDatabase();
        var apiKeyJson = await db.StringGetAsync($"api_key:{key}");

        if (!apiKeyJson.HasValue)
        {
            return null;
        }

        var apiKey = JsonConvert.DeserializeObject<ApiKey>(apiKeyJson!);

        if (apiKey is not { IsActive: true } || apiKey.ExpiresAt < DateTime.UtcNow)
        {
            return null;
        }

        return apiKey;
    }

    public async Task<List<ApiKey>> GetUserApiKeysAsync(string userId)
    {
        var db = redis.GetDatabase();
        var apiKeyIds = await db.SetMembersAsync($"user_api_keys:{userId}");

        if (apiKeyIds.Length == 0)
        {
            return [];
        }

        var tasks = apiKeyIds.Select(async keyId =>
        {
            var apiKeyJson = await db.StringGetAsync($"api_key:{keyId}");
            return apiKeyJson.HasValue ? JsonConvert.DeserializeObject<ApiKey>(apiKeyJson!)! : null;
        });

        var results = await Task.WhenAll(tasks);
        return results
            .Where(k => k is { IsActive: true } && k.ExpiresAt > DateTime.UtcNow)
            .ToList()!;
    }

    public async Task RevokeApiKeyAsync(string key)
    {
        var db = redis.GetDatabase();
        var apiKeyJson = await db.StringGetAsync($"api_key:{key}");

        if (!apiKeyJson.HasValue)
        {
            return;
        }

        var apiKey = JsonConvert.DeserializeObject<ApiKey>(apiKeyJson!);

        if (apiKey != null)
        {
            apiKey.IsActive = false;
            await db.StringSetAsync(
                $"api_key:{key}",
                JsonConvert.SerializeObject(apiKey),
                TimeSpan.FromDays((apiKey.ExpiresAt - DateTime.UtcNow).TotalDays)
            );
        }
    }
}
