using Microsoft.Extensions.Options;
using tracksByPopularity.configuration;

namespace tracksByPopularity.services;

/// <summary>
/// Service implementation for accessing application configuration.
/// Wraps IOptions to provide direct access to configuration values.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ConfigurationService"/> class.
    /// </summary>
    /// <param name="appSettingsOptions">The application settings options.</param>
    /// <param name="spotifySettingsOptions">The Spotify settings options.</param>
    /// <param name="playlistSettingsOptions">The playlist settings options.</param>
    /// <param name="redisSettingsOptions">The Redis settings options.</param>
    public ConfigurationService(
        IOptions<AppSettings> appSettingsOptions,
        IOptions<SpotifySettings> spotifySettingsOptions,
        IOptions<PlaylistSettings> playlistSettingsOptions,
        IOptions<RedisSettings> redisSettingsOptions
    )
    {
        AppSettings = appSettingsOptions.Value;
        SpotifySettings = spotifySettingsOptions.Value;
        PlaylistSettings = playlistSettingsOptions.Value;
        RedisSettings = redisSettingsOptions.Value;
    }

    /// <inheritdoc />
    public AppSettings AppSettings { get; }

    /// <inheritdoc />
    public SpotifySettings SpotifySettings { get; }

    /// <inheritdoc />
    public PlaylistSettings PlaylistSettings { get; }

    /// <inheritdoc />
    public RedisSettings RedisSettings { get; }
}

