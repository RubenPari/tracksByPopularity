using dotenv.net;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using StackExchange.Redis;
using System.Text;
using tracksByPopularity.Infrastructure.Background;
using tracksByPopularity.Infrastructure.Logging;
using tracksByPopularity.Infrastructure.Data;
using tracksByPopularity.Presentation.Middlewares;
using Microsoft.Extensions.Options;

DotEnv.Load();

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog for logging
builder.Host.UseSerilog((context, services, loggerConfig) =>
{
    loggerConfig
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .MinimumLevel.Debug()
        .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Information)
        .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
        .MinimumLevel.Override("System", Serilog.Events.LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .WriteTo.Console(
            outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
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

// Configure JWT Authentication
var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET") ?? "default-secret-key-change-in-production-min-32-chars-long";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
        ValidateIssuer = true,
        ValidIssuer = "tracksByPopularity",
        ValidateAudience = true,
        ValidAudience = "tracksByPopularity",
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
    };
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Cookies["access_token"];
            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Token = accessToken;
            }
            return Task.CompletedTask;
        }
    };
});

// Configure MySQL Database
var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") 
    ?? "Server=localhost;Port=3306;Database=tracksbypopularity;User=root;Password=password;";
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySql(connectionString, new MariaDbServerVersion(new Version(11, 0))));

// Register services
builder.Services.AddSingleton<IJwtService, JwtService>();
builder.Services.AddScoped<IEmailService, MailtrapEmailService>();
builder.Services.AddScoped<IAccountAuthService, AccountAuthService>();

// Add controllers with FluentValidation and JSON options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
        options.JsonSerializerOptions.DictionaryKeyPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddFluentValidationClientsideAdapters();
builder.Services.AddValidatorsFromAssemblyContaining<AddTracksByArtistRequestValidator>();

// Service that set Redis cache
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var redisSettings = sp.GetRequiredService<IOptions<RedisSettings>>().Value;

    var configuration = new ConfigurationOptions
    {
        EndPoints = { $"{redisSettings.Host}:{redisSettings.Port}" },
        Password = redisSettings.Password,
        Ssl = redisSettings.UseSsl,
        AllowAdmin = false, // Disabled for security, use pattern-based deletion instead
        AbortOnConnectFail = false,
        ConnectTimeout = 10000, // 10 seconds
        SyncTimeout = 10000, // 10 seconds
        AsyncTimeout = 10000, // 10 seconds
        ReconnectRetryPolicy = new LinearRetry(5000), // 5 seconds retry
        ConnectRetry = 3,
        KeepAlive = 60, // Send keepalive every 60 seconds
    };

    return ConnectionMultiplexer.Connect(configuration);
});

// Add response caching for improved performance
builder.Services.AddResponseCaching();
builder.Services.AddHostedService<RedisCacheResetService>();
builder.Services.AddHostedService<SnapshotCleanupService>();

// Register services
builder.Services.AddScoped<SpotifyAuthService>();
builder.Services.AddScoped<ITrackService, TrackService>();
builder.Services.AddScoped<IPlaylistService, PlaylistService>();
builder.Services.AddScoped<IArtistService, ArtistService>();
builder.Services.AddScoped<IPlaylistHelper, PlaylistHelperService>();
builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
builder.Services.AddScoped<ITrackCategorizationService, TrackCategorizationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<ITrackOrganizationService, TrackOrganizationService>();
builder.Services.AddScoped<IArtistTrackOrganizationService, ArtistTrackOrganizationService>();
builder.Services.AddScoped<IPlaylistBackupService, PlaylistBackupService>();

// Register cache infrastructure (DIP: depends on abstraction ICacheRepository)
builder.Services.AddSingleton<ICacheRepository, RedisCacheRepository>();

// Register cache services (ISP: separate interfaces for each cache type)
builder.Services.AddScoped<ITrackCacheService, TrackCacheService>();
builder.Services.AddScoped<IPlaylistCacheService, PlaylistCacheService>();
builder.Services.AddScoped<IArtistCacheService, ArtistCacheService>();

builder.Services.AddHttpClient();

var app = builder.Build();

// Apply pending EF Core migrations automatically
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate();
}

// Add global exception handling middleware
app.UseGlobalExceptionHandling();

// Add response caching middleware
app.UseResponseCaching();

// Add authentication and authorization
app.UseAuthentication();
app.UseAuthorization();

// Add other middlewares
app.UseMiddleware<RedirectHomeMiddleware>();

// Map controllers (standard ASP.NET Core controllers, not minimal API)
app.MapControllers();

app.Run();

// Ensure Serilog flushes on application shutdown
Log.CloseAndFlush();
