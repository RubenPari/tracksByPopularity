using dotenv.net;
using StackExchange.Redis;
using tracksByPopularity.background;
using tracksByPopularity.middlewares;
using tracksByPopularity.routes;
using tracksByPopularity.services;
using tracksByPopularity.utils;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();

// Add controllers
builder.Services.AddControllers();

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

builder.Services.AddHttpClient();

var app = builder.Build();

// Add global exception handler middleware first
app.UseMiddleware<GlobalExceptionHandlerMiddleware>();

// Add other middlewares
app.UseMiddleware<RedirectHomeMiddleware>();
app.UseMiddleware<ClearPlaylistMiddleware>();

// Map controllers
app.MapControllers();

// Keep legacy routes for backward compatibility
Routes.MapRoutes(app);

app.Run();
