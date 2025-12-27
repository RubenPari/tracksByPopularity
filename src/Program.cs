using dotenv.net;
using FluentValidation;
using FluentValidation.AspNetCore;
using StackExchange.Redis;
using tracksByPopularity.background;
using tracksByPopularity.middlewares;
using tracksByPopularity.routes;
using tracksByPopularity.services;
using tracksByPopularity.utils;
using tracksByPopularity.validators;
using tracksByPopularity.configuration;

var builder = WebApplication.CreateBuilder(args);

DotEnv.Load();

// Configure application settings
builder.Services.AddApplicationConfiguration(builder.Configuration);

// Add controllers with FluentValidation
builder.Services.AddControllers()
    .AddFluentValidation(fv =>
    {
        fv.RegisterValidatorsFromAssemblyContaining<AddTracksByArtistRequestValidator>();
        fv.AutomaticValidationEnabled = true;
        fv.ImplicitlyValidateChildProperties = true;
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
builder.Services.AddScoped<domain.services.ITrackCategorizationService, domain.services.TrackCategorizationService>();

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
