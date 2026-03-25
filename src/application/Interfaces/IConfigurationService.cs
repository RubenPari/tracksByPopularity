namespace tracksByPopularity.Application.Interfaces;

/// <summary>
/// Service interface for accessing application configuration.
/// Provides strongly-typed access to configuration settings.
/// </summary>
public interface IConfigurationService
{
    // TODO: implementati ma non usati
    /// <summary>
    /// Gets the application settings.
    /// </summary>
    AppSettings AppSettings { get; }

    // TODO: implementati ma non usati
    /// <summary>
    /// Gets the Spotify API settings.
    /// </summary>
    SpotifySettings SpotifySettings { get; }

    // TODO: implementati ma non usati
    /// <summary>
    /// Gets the Redis cache settings.
    /// </summary>
    RedisSettings RedisSettings { get; }
}

