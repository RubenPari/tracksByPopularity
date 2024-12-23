using dotenv.net;
using StackExchange.Redis;
using tracksByPopularity.background;
using tracksByPopularity.middlewares;
using tracksByPopularity.routes;
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

builder.Services.AddHttpClient();

var app = builder.Build();

// Add middlewares
app.UseMiddleware<RedirectHomeMiddleware>();
app.UseMiddleware<CheckAuthMiddleware>();
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
