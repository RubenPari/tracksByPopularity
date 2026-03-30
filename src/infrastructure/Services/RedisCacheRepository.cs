using System.IO.Compression;
using System.Text;
using Newtonsoft.Json;
using StackExchange.Redis;
using tracksByPopularity.Application.Interfaces;

namespace tracksByPopularity.Infrastructure.Services;

/// <summary>
/// Redis implementation of ICacheRepository with compression support.
/// Follows DIP: Depends on IConnectionMultiplexer abstraction from StackExchange.Redis.
/// Performance: Compresses data larger than 1KB to reduce memory usage.
/// </summary>
public class RedisCacheRepository : ICacheRepository
{
    private readonly IConnectionMultiplexer _connection;
    private const int CompressionThreshold = 1024; // Compress data larger than 1KB
    private const byte CompressionMarker = 0x1F; // Marker byte to identify compressed data

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

        // Check if data is compressed
        var bytes = (byte[])value!;
        if (bytes.Length > 0 && bytes[0] == CompressionMarker)
        {
            // Decompress
            var decompressed = Decompress(bytes.AsSpan(1));
            return JsonConvert.DeserializeObject<T>(decompressed);
        }

        return JsonConvert.DeserializeObject<T>(value!);
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class
    {
        var db = _connection.GetDatabase();
        var serialized = JsonConvert.SerializeObject(value);
        var bytes = Encoding.UTF8.GetBytes(serialized);

        // Compress if larger than threshold
        byte[] dataToStore;
        if (bytes.Length > CompressionThreshold)
        {
            dataToStore = Compress(bytes);
        }
        else
        {
            dataToStore = bytes;
        }

        await db.StringSetAsync(key, dataToStore, expiration);
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
