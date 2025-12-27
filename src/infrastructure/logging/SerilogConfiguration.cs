using Serilog;
using Serilog.Events;
using Serilog.Formatting.Compact;

namespace tracksByPopularity.infrastructure.logging;

/// <summary>
/// Provides configuration for Serilog logging infrastructure.
/// </summary>
public static class SerilogConfiguration
{
    /// <summary>
    /// Configures and creates a Serilog logger instance.
    /// </summary>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>A configured Serilog logger.</returns>
    public static LoggerConfiguration CreateLoggerConfiguration(IConfiguration configuration)
    {
        var loggerConfiguration = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
            .MinimumLevel.Override("System", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithEnvironmentName()
            .Enrich.WithMachineName()
            .Enrich.WithThreadId()
            .Enrich.WithProperty("Application", "TracksByPopularity")
            .WriteTo.Console(
                outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}"
            )
            .WriteTo.File(
                formatter: new CompactJsonFormatter(),
                path: "logs/tracks-by-popularity-.log",
                rollingInterval: RollingInterval.Day,
                retainedFileCountLimit: 30,
                shared: true
            );

        // Allow configuration override from appsettings.json
        loggerConfiguration.ReadFrom.Configuration(configuration);

        return loggerConfiguration;
    }
}

