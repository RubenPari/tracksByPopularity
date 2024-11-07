using dotenv.net;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity.background;
using tracksByPopularity.helpers;
using tracksByPopularity.middlewares;
using tracksByPopularity.routes;
using tracksByPopularity.services;
using tracksByPopularity.services.Track;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();

// Set Redis cache service
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = ConfigurationOptions.Parse(Constants.RedisConnectionString);

    configuration.AllowAdmin = true;
    configuration.AbortOnConnectFail = false;
    configuration.ConnectTimeout = 20000; // 20 seconds
    configuration.SyncTimeout = 20000; // 20 seconds
    configuration.AsyncTimeout = 20000; // 20 seconds
    configuration.ReconnectRetryPolicy = new LinearRetry(10000); // 10 seconds

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddHostedService<RedisCacheResetService>();

// Set Spotify client injector
builder.Services.AddSingleton<SpotifyAuthService>();

builder.Services.AddScoped<TrackService>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Add middlewares
app.UseMiddleware<CheckAuthMiddleware>();
app.UseMiddleware<ClearPlaylistMiddleware>();

DotEnv.Load();

Routes.MapRoutes(app);

app.Run();