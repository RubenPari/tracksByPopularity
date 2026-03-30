namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Abstraction for cache storage operations.
/// Follows DIP: High-level modules (services) depend on this abstraction, not on concrete implementations.
/// </summary>
public interface ICacheRepository
{
    /// <summary>
    /// Gets a cached value by key.
    /// </summary>
    /// <typeparam name="T">The type of the cached value.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <returns>The cached value or default if not found.</returns>
    Task<T?> GetAsync<T>(string key) where T : class;

    /// <summary>
    /// Sets a cached value with expiration.
    /// </summary>
    /// <typeparam name="T">The type of the value to cache.</typeparam>
    /// <param name="key">The cache key.</param>
    /// <param name="value">The value to cache.</param>
    /// <param name="expiration">Time until the cache expires.</param>
    Task SetAsync<T>(string key, T value, TimeSpan expiration) where T : class;

    /// <summary>
    /// Removes a cached value.
    /// </summary>
    /// <param name="key">The cache key to remove.</param>
    Task RemoveAsync(string key);

    /// <summary>
    /// Checks if a key exists in cache.
    /// </summary>
    /// <param name="key">The cache key.</param>
    Task<bool> ExistsAsync(string key);
}
