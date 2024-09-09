using dotenv.net;
using StackExchange.Redis;
using tracksByPopularity.src;
using tracksByPopularity.src.background;
using tracksByPopularity.src.middlewares;
using tracksByPopularity.src.routes;

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

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddHostedService<RedisCacheResetService>();

var app = builder.Build();

// Add middlewares
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