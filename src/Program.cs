using dotenv.net;
using StackExchange.Redis;
using tracksByPopularity.background;
using tracksByPopularity.middlewares;
using tracksByPopularity.routes;
using tracksByPopularity.services;
using tracksByPopularity.utils;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = Constants.TitleApi;
    config.Title = Constants.TitleApi;
    config.Version = "v1";
});

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
        ReconnectRetryPolicy = new LinearRetry(10000) // 10 seconds
    };

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddHostedService<RedisCacheResetService>();

builder.Services.AddScoped<ApiKeyService>();
builder.Services.AddScoped<SpotifyAuthService>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Add middlewares
app.UseMiddleware<RedirectHomeMiddleware>();
app.UseMiddleware<ApiKeyAuthMiddleware>();
app.UseMiddleware<ClearPlaylistMiddleware>();

// Add services to use OpenApi and Swagger UI
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = Constants.TitleApi;
    config.DocExpansion = "fully";
});

DotEnv.Load();

Routes.MapRoutes(app);

app.Run();
