using dotenv.net;
using StackExchange.Redis;
using tracksByPopularity;
using tracksByPopularity.background;
using tracksByPopularity.controllers;
using tracksByPopularity.middlewares;

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
app.UseMiddleware<RedirectHomeMiddleware>();
app.UseMiddleware<CheckAuthMiddleware>();
app.UseMiddleware<ClearPlaylistMiddleware>();

// Add services to use OpenApi and Swagger UI
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = Constants.TitleApi;
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

DotEnv.Load();

// map controllers

// ####### /AUTH #######

var authRoutes = app.MapGroup("/auth");

authRoutes.MapGet("/login", AuthController.Login);

authRoutes.MapGet("/callback", AuthController.Callback);

authRoutes.MapGet("/logout", AuthController.Logout);

// ####### /TRACK #######

var trackRoutes = app.MapGroup("/track");

trackRoutes.MapPost("/top", TrackController.Top);

trackRoutes.MapPost("/less", TrackController.Less);

trackRoutes.MapPost("/less-medium", TrackController.LessMedium);

trackRoutes.MapPost("/more-medium", TrackController.MoreMedium);

trackRoutes.MapPost("/more", TrackController.More);

trackRoutes.MapPost("/artist", TrackController.Artist);

app.Run();
