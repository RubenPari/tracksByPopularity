using dotenv.net;
using FluentValidation;
using FluentValidation.AspNetCore;
using Serilog;
using StackExchange.Redis;
using tracksByPopularity.application.services;
using tracksByPopularity.background;
using tracksByPopularity.domain.services;
using tracksByPopularity.infrastructure.logging;
using tracksByPopularity.middlewares;
using tracksByPopularity.services;
using tracksByPopularity.utils;
using tracksByPopularity.validators;
using tracksByPopularity.configuration;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
builder.Host.UseSerilog((context, services, configuration) =>
{
    SerilogConfiguration.CreateLoggerConfiguration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .WriteTo.Console()
        .WriteTo.File(
            formatter: new Serilog.Formatting.Compact.CompactJsonFormatter(),
            path: "logs/tracks-by-popularity-.log",
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            shared: true
        );
});

// Configure application settings
builder.Services.AddApplicationConfiguration(builder.Configuration);

// Add controllers with FluentValidation
builder.Services.AddControllers();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<AddTracksByArtistRequestValidator>();

// Service that set Redis cache
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = new ConfigurationOptions
    {
        EndPoints = { Constants.RedisHost + ":" + Constants.RedisPort },
        Password = Constants.RedisPassword,
        Ssl = true,
        AllowAdmin = true,
        AbortOnConnectFail = false,
        ConnectTimeout = 20000, // 20 seconds
        SyncTimeout = 20000, // 20 seconds
        AsyncTimeout = 20000, // 20 seconds
        ReconnectRetryPolicy = new LinearRetry(10000), // 10 seconds
    };

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddHostedService<RedisCacheResetService>();

// Register services
builder.Services.AddScoped<SpotifyAuthService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IPlaylistHelper, PlaylistHelperService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ITrackCategorizationService, TrackCategorizationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IPlaylistRoutingService, PlaylistRoutingService>();
builder.Services.AddScoped<IPlaylistClearingService, PlaylistClearingService>();
builder.Services.AddScoped<ITrackOrganizationService, TrackOrganizationService>();
builder.Services.AddScoped<IArtistTrackOrganizationService, ArtistTrackOrganizationService>();
builder.Services.AddScoped<IMinorSongsPlaylistService, MinorSongsPlaylistService>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Add global exception handler middleware first
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Add other middlewares
app.UseMiddleware<RedirectHomeMiddleware>();
app.UseMiddleware<ClearPlaylistMiddleware>();

// Map controllers (standard ASP.NET Core controllers, not minimal API)
app.MapControllers();

app.Run();

// Ensure Serilog flushes on application shutdown
Log.CloseAndFlush();
