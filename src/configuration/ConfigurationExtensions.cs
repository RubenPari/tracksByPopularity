using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using tracksByPopularity.configuration;

namespace tracksByPopularity.configuration;

/// <summary>
/// Extension methods for configuring application settings using IOptions pattern.
/// </summary>
public static class ConfigurationExtensions
{
    /// <summary>
    /// Configures application settings from configuration sources.
    /// </summary>
    /// <param name="services">The service collection to add configuration to.</param>
    /// <param name="configuration">The configuration instance.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApplicationConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        // Bind configuration sections to strongly-typed settings classes
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<SpotifySettings>(configuration.GetSection("SpotifySettings"));
        services.Configure<PlaylistSettings>(configuration.GetSection("PlaylistSettings"));
        services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));

        // Also bind from environment variables as fallback
        services.Configure<AppSettings>(options =>
        {
            // AppSettings can be overridden by environment variables if needed
        });

        services.Configure<SpotifySettings>(options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? options.ClientId;
            options.ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? options.ClientSecret;
            options.RedirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? options.RedirectUri;
        });

        services.Configure<PlaylistSettings>(options =>
        {
            options.PlaylistIdLess = Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS") ?? options.PlaylistIdLess;
            options.PlaylistIdLessMedium = Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS_MEDIUM") ?? options.PlaylistIdLessMedium;
            options.PlaylistIdMedium = Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM") ?? options.PlaylistIdMedium;
            options.PlaylistIdMoreMedium = Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE_MEDIUM") ?? options.PlaylistIdMoreMedium;
            options.PlaylistIdMore = Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE") ?? options.PlaylistIdMore;
            options.PlaylistIdTopShort = Environment.GetEnvironmentVariable("PLAYLIST_ID_TOP_SHORT") ?? options.PlaylistIdTopShort;
            options.PlaylistIdTopMedium = Environment.GetEnvironmentVariable("PLAYLIST_ID_TOP_MEDIUM") ?? options.PlaylistIdTopMedium;
            options.PlaylistIdTopLong = Environment.GetEnvironmentVariable("PLAYLIST_ID_TOP_LONG") ?? options.PlaylistIdTopLong;
        });

        services.Configure<RedisSettings>(options =>
        {
            options.Host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? options.Host;
            options.Port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? options.Port;
            options.Password = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? options.Password;
        });

        return services;
    }
}

