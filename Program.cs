using dotenv.net;
using SpotifyAPI.Web;
using StackExchange.Redis;
using tracksByPopularity;
using tracksByPopularity.background;
using tracksByPopularity.helpers;
using tracksByPopularity.middlewares;
using tracksByPopularity.models;
using tracksByPopularity.services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(config =>
{
    config.DocumentName = Costants.TitleApi;
    config.Title = Costants.TitleApi;
    config.Version = "v1";
});

// Service that set Redis cache
builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
{
    var configuration = ConfigurationOptions.Parse(Costants.RedisConnectionString);

    configuration.AllowAdmin = true;
    configuration.AbortOnConnectFail = false;

    return ConnectionMultiplexer.Connect(configuration);
});
builder.Services.AddHostedService<RedisCacheResetService>();

var app = builder.Build();

// Add middlewares
app.UseMiddleware<RedirectHomeMiddleware>();
app.UseMiddleware<CheckAuthMiddleware>();

// Add services to use OpenApi and Swagger UI
app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = Costants.TitleApi;
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

DotEnv.Load();

// ####### /AUTH #######

var authRoutes = app.MapGroup("/auth");

authRoutes.MapGet(
    "/login",
    () =>
    {
        var request = new LoginRequest(
            new Uri(Costants.RedirectUri),
            Costants.ClientId,
            LoginRequest.ResponseType.Code
        )
        {
            Scope = Costants.MyScopes
        };

        var uri = request.ToUri();

        return Results.Redirect(uri.ToString());
    }
);

authRoutes.MapGet(
    "/callback",
    async (string code) =>
    {
        var response = await new OAuthClient().RequestToken(
            new AuthorizationCodeTokenRequest(
                Costants.ClientId,
                Costants.ClientSecret,
                code,
                new Uri(Costants.RedirectUri)
            )
        );

        Client.Spotify = new SpotifyClient(Costants.Config.WithToken(response.AccessToken));

        var user = await Client.Spotify.UserProfile.Current();

        return user.Id == string.Empty
            ? Results.BadRequest("Login failed, retry")
            : Results.Ok("Successfully authenticated!");
    }
);

authRoutes.MapGet(
    "/logout",
    () =>
    {
        Client.Spotify = null;

        return Results.Ok("Successfully logged out!");
    }
);

// ####### /TRACK #######

var trackRoutes = app.MapGroup("/track");

trackRoutes.MapPost(
    "/top",
    async (HttpContext httpContext, IConnectionMultiplexer cacheRedisConnection) =>
    {
        var timeRangeString = httpContext.Request.Query["timeRange"].FirstOrDefault();

        if (string.IsNullOrEmpty(timeRangeString))
        {
            return Results.BadRequest("Time range is required");
        }

        // convert timeRange from string to enum
        var timeRangeEnum = timeRangeString.ToEnum<TimeRangeEnum>();

        if (timeRangeEnum == null)
        {
            return Results.BadRequest("Invalid time range");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var topTracks = await TrackService.GetTopTracks(timeRangeEnum.Value, allTracks);

        // convert list of FullTrack to SavedTrack
        var tracks = topTracks.Select(track => new SavedTrack { Track = track }).ToList();

        // added tracks to playlist based on time range
        bool addedToPlaylist = false;

        switch (timeRangeEnum)
        {
            case TimeRangeEnum.ShortTerm:
                addedToPlaylist = await TrackService.AddTracksToPlaylist(
                    Costants.PlaylistIdTopShort,
                    tracks!
                );
                break;
            case TimeRangeEnum.MediumTerm:
                addedToPlaylist = await TrackService.AddTracksToPlaylist(
                    Costants.PlaylistIdTopMedium,
                    tracks!
                );
                break;
            case TimeRangeEnum.LongTerm:
                addedToPlaylist = await TrackService.AddTracksToPlaylist(
                    Costants.PlaylistIdTopLong,
                    tracks!
                );
                break;
        }

        return addedToPlaylist
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }
);

trackRoutes.MapPost(
    "/less",
    async (IConnectionMultiplexer cacheRedisConnection) =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdLess))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdLess))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track => track.Track.Popularity <= Costants.TracksLessPopularity)
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdLess,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }
);

trackRoutes.MapPost(
    "/less-medium",
    async (IConnectionMultiplexer cacheRedisConnection) =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdLessMedium))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdLessMedium))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessPopularity
                && track.Track.Popularity <= Costants.TracksLessMediumPopularity
            )
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdLessMedium,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }
);

trackRoutes.MapPost(
    "/more-medium",
    async (IConnectionMultiplexer cacheRedisConnection) =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdMoreMedium))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdMoreMedium))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessMediumPopularity
                && track.Track.Popularity <= Costants.TracksMoreMediumPopularity
            )
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdMoreMedium,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }
);

trackRoutes.MapPost(
    "/more",
    async (IConnectionMultiplexer cacheRedisConnection) =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdMore))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdMore))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var trackWithPopularity = allTracks
            .Where(track => track.Track.Popularity > Costants.TracksMoreMediumPopularity)
            .ToList();

        var added = await TrackService.AddTracksToPlaylist(
            Costants.PlaylistIdMore,
            trackWithPopularity
        );

        return added
            ? Results.Ok("Tracks added to playlist")
            : Results.BadRequest("Failed to add tracks to playlist");
    }
);

trackRoutes.MapPost(
    "/artist",
    async (
        string artistId,
        IdArtistPlaylistsBody idArtistPlaylistsBody,
        IConnectionMultiplexer cacheRedisConnection
    ) =>
    {
        // check if playlists are valid
        if (
            !await PlaylistHelper.CheckValidityPlaylist(
                idArtistPlaylistsBody.Less,
                idArtistPlaylistsBody.LessMedium,
                idArtistPlaylistsBody.MoreMedium,
                idArtistPlaylistsBody.More
            )
        )
        {
            return Results.BadRequest("Playlist not found");
        }

        // check if playlists are empty
        if (
            !await PlaylistHelper.CheckIsEmptyPlaylist(
                idArtistPlaylistsBody.Less,
                idArtistPlaylistsBody.LessMedium,
                idArtistPlaylistsBody.MoreMedium,
                idArtistPlaylistsBody.More
            )
        )
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        // get all user tracks, if possible from cache
        var allTracks = await CacheHelper.GetAllUserTracks(cacheRedisConnection);

        var allTracksArtist = allTracks
            .Where(track => track.Track.Artists.Any(artist => artist.Id == artistId))
            .ToList();

        var trackWithLessPopularity = allTracksArtist
            .Where(track => track.Track.Popularity <= Costants.TracksLessPopularity)
            .ToList();

        var trackWithLessMediumPopularity = allTracksArtist
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessPopularity
                && track.Track.Popularity <= Costants.TracksLessMediumPopularity
            )
            .ToList();

        var trackWithMoreMediumPopularity = allTracksArtist
            .Where(track =>
                track.Track.Popularity > Costants.TracksLessMediumPopularity
                && track.Track.Popularity <= Costants.TracksMoreMediumPopularity
            )
            .ToList();

        var trackWithMorePopularity = allTracksArtist
            .Where(track => track.Track.Popularity > Costants.TracksMoreMediumPopularity)
            .ToList();

        var addedLess = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.Less,
            trackWithLessPopularity
        );

        var addedLessMedium = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.LessMedium,
            trackWithLessMediumPopularity
        );

        var addedMoreMedium = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.MoreMedium,
            trackWithMoreMediumPopularity
        );

        var addedMore = await TrackService.AddTracksToPlaylist(
            idArtistPlaylistsBody.More,
            trackWithMorePopularity
        );

        if (!addedLess || !addedLessMedium || !addedMoreMedium || !addedMore)
        {
            return Results.BadRequest("Failed to add tracks to playlist");
        }

        return Results.Ok("Tracks added to playlist");
    }
);

app.Run();
