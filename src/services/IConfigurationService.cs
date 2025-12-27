using tracksByPopularity.configuration;

namespace tracksByPopularity.services;

/// <summary>
/// Service interface for accessing application configuration.
/// Provides strongly-typed access to configuration settings.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Gets the application settings.
    /// </summary>
    AppSettings AppSettings { get; }

    /// <summary>
    /// Gets the Spotify API settings.
    /// </summary>
    SpotifySettings SpotifySettings { get; }

    /// <summary>
    /// Gets the playlist settings.
    /// </summary>
    PlaylistSettings PlaylistSettings { get; }

    /// <summary>
    /// Gets the Redis cache settings.
    /// </summary>
    RedisSettings RedisSettings { get; }
}

