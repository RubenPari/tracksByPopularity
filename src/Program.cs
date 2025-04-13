using System.Text;
using dotenv.net;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
using tracksByPopularity.background;
using tracksByPopularity.controllers;
using tracksByPopularity.middlewares;
using tracksByPopularity.models;
using tracksByPopularity.routes;
using tracksByPopularity.utils;

var builder = WebApplication.CreateBuilder(args);

// Load environment variables early
DotEnv.Load();

// Add JWT configuration from app settings
builder
    .Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    builder.Configuration["Jwt:Key"]
                        ?? throw new ArgumentNullException("JWT Key is missing")
                )
            ),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero,
        };
    });

builder.Services.AddAuthorization();

// Register JWT service
builder.Services.AddSingleton<ITokenService, TokenService>();

// Register HttpContextAccessor for accessing the current HTTP context
builder.Services.AddHttpContextAccessor();

// Register Spotify client accessor
builder.Services.AddScoped<ISpotifyClientAccessor, SpotifyClientAccessor>();

// Register AuthController as a service because it needs dependencies
builder.Services.AddScoped<AuthController>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = Constants.TitleApi;
    config.Title = Constants.TitleApi;
    config.Version = "v1";

    // Add JWT authentication to Swagger
    config.AddSecurity(
        "Bearer",
        new NSwag.OpenApiSecurityScheme
        {
            Description =
                "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
            Name = "Authorization",
            In = NSwag.OpenApiSecurityApiKeyLocation.Header,
            Type = NSwag.OpenApiSecuritySchemeType.ApiKey,
        }
    );
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
app.UseMiddleware<JwtAuthMiddleware>(); // Replace CheckAuthMiddleware with JwtAuthMiddleware
app.UseMiddleware<ClearPlaylistMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

// Add services to use OpenApi and Swagger UI
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = Constants.TitleApi;
    config.DocExpansion = "fully";
});

Routes.MapRoutes(app);

app.Run();
