using System.IO.Compression;
using System.Text;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Polly;
using Polly.Retry;
using StackExchange.Redis;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Redis implementation of ICacheRepository with compression and retry support.
/// Follows DIP: Depends on IConnectionMultiplexer abstraction from StackExchange.Redis.
/// Performance: 
/// - Compresses data larger than 1KB to reduce memory usage
/// - Retries on transient failures with exponential backoff
/// </summary>
public class RedisCacheRepository : ICacheRepository
{
    private readonly IConnectionMultiplexer _connection;
    private readonly ILogger<RedisCacheRepository> _logger;
    private readonly ResiliencePipeline _retryPipeline;
    private const int CompressionThreshold = 1024; // Compress data larger than 1KB
    private const byte CompressionMarker = 0x1F; // Marker byte to identify compressed data

    public RedisCacheRepository(IConnectionMultiplexer connection, ILogger<RedisCacheRepository> logger)
    {
        _connection = connection;
        _logger = logger;
        
        // Configure retry pipeline with exponential backoff
        _retryPipeline = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                MaxRetryAttempts = 3,
                Delay = TimeSpan.FromMilliseconds(100),
                BackoffType = DelayBackoffType.Exponential,
                ShouldHandle = new PredicateBuilder().Handle<RedisConnectionException>()
                    .Handle<RedisTimeoutException>(),
                OnRetry = args =>
                {
                    _logger.LogWarning("Redis retry attempt {AttemptNumber} after {Delay}ms", 
                        args.AttemptNumber, args.RetryDelay.TotalMilliseconds);
                    return ValueTask.CompletedTask;
                }
            })
            .Build();
    }

    public async Task<T?> GetAsync<T>(string key) where T : class
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
        {
            var db = _connection.GetDatabase();
            var value = await db.StringGetAsync(key);
            
            if (!value.HasValue)
            {
                return null;
            }

            // Check if data is compressed
            var bytes = (byte[])value!;
            if (bytes.Length > 0 && bytes[0] == CompressionMarker)
            {
                // Decompress
                var decompressed = Decompress(bytes.AsSpan(1));
                return JsonConvert.DeserializeObject<T>(decompressed);
            }

            return JsonConvert.DeserializeObject<T>(value!);
        });
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        await _retryPipeline.ExecuteAsync(async ct =>
        {
            var db = _connection.GetDatabase();
            var serialized = JsonConvert.SerializeObject(value);
            var bytes = Encoding.UTF8.GetBytes(serialized);

            // Compress if larger than threshold
            byte[] dataToStore;
            if (bytes.Length > CompressionThreshold)
            {
                dataToStore = Compress(bytes);
                _logger.LogDebug("Compressed cache data for key {Key}: {OriginalSize} -> {CompressedSize} bytes", 
                    key, bytes.Length, dataToStore.Length);
            }
            else
            {
                dataToStore = bytes;
            }

            await db.StringSetAsync(key, dataToStore, expiration);
        });
    }

    public async Task RemoveAsync(string key)
    {
        await _retryPipeline.ExecuteAsync(async ct =>
        {
            var db = _connection.GetDatabase();
            await db.KeyDeleteAsync(key);
        });
    }

    public async Task<bool> ExistsAsync(string key)
    {
        return await _retryPipeline.ExecuteAsync(async ct =>
        {
            var db = _connection.GetDatabase();
            return await db.KeyExistsAsync(key);
        });
    }

    private static byte[] Compress(ReadOnlySpan<byte> data)
    {
        using var output = new MemoryStream();
        output.WriteByte(CompressionMarker); // Write compression marker
        
        using (var gzip = new GZipStream(output, CompressionLevel.Optimal))
        {
            gzip.Write(data);
        }
        
        return output.ToArray();
    }

    private static string Decompress(ReadOnlySpan<byte> compressedData)
    {
        using var input = new MemoryStream(compressedData.ToArray());
        using var gzip = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        gzip.CopyTo(output);
        return Encoding.UTF8.GetString(output.ToArray());
    }
}
