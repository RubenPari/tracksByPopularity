namespace tracksByPopularity.Infrastructure.Configuration;

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
    public static void AddApplicationConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration sections to strongly-typed settings classes
        services.Configure<AppSettings>(configuration.GetSection("AppSettings"));
        services.Configure<SpotifySettings>(configuration.GetSection("SpotifySettings"));
        services.Configure<RedisSettings>(configuration.GetSection("RedisSettings"));
        services.Configure<JwtSettings>(configuration.GetSection("JwtSettings"));
        services.Configure<MailtrapSettings>(configuration.GetSection("MailtrapSettings"));
        services.Configure<DatabaseSettings>(configuration.GetSection("DatabaseSettings"));

        // Also bind from environment variables as fallback
        services.Configure<AppSettings>(_ =>
        {
            // AppSettings can be overridden by environment variables if needed
        });

        services.Configure<SpotifySettings>(options =>
        {
            options.ClientId = Environment.GetEnvironmentVariable("CLIENT_ID") ?? options.ClientId;
            options.ClientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET") ?? options.ClientSecret;
            options.RedirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI") ?? options.RedirectUri;
        });

        services.Configure<RedisSettings>(options =>
        {
            options.Host = Environment.GetEnvironmentVariable("REDIS_HOST") ?? options.Host;
            options.Port = Environment.GetEnvironmentVariable("REDIS_PORT") ?? options.Port;
            options.Password = Environment.GetEnvironmentVariable("REDIS_PASSWORD") ?? options.Password;
        });

        services.Configure<JwtSettings>(options =>
        {
            options.Secret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? options.Secret;
            options.Issuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? options.Issuer;
            options.Audience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? options.Audience;
            if (int.TryParse(Environment.GetEnvironmentVariable("JWT_EXPIRY_DAYS"), out var expiryDays))
                options.ExpiryDays = expiryDays;
        });

        services.Configure<MailtrapSettings>(options =>
        {
            options.ApiKey = Environment.GetEnvironmentVariable("MAILTRAP_API_KEY") ?? options.ApiKey;
            options.ClientUrl = Environment.GetEnvironmentVariable("CLIENT_URL") ?? options.ClientUrl;
        });

        services.Configure<DatabaseSettings>(options =>
        {
            options.ConnectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? options.ConnectionString;
        });
    }
}

