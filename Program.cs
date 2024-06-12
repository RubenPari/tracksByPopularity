using dotenv.net;
using SpotifyAPI.Web;
using tracksByPopularity;
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

var app = builder.Build();

app.UseMiddleware<CheckAuthMiddleware>();

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = Costants.TitleApi;
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

DotEnv.Load();

app.MapGet(
    "/auth/login",
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

app.MapGet(
    "/auth/callback",
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

app.MapGet(
    "/auth/logout",
    () =>
    {
        Client.Spotify = null;

        return Results.Ok("Successfully logged out!");
    }
);

app.MapPost(
    "/track/less",
    async () =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdLess))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdLess))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        var allTracks = await TrackService.GetAllUserTracks();

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

app.MapPost(
    "/track/less-medium",
    async () =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdLessMedium))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdLessMedium))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        var allTracks = await TrackService.GetAllUserTracks();

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

app.MapPost(
    "/track/more-medium",
    async () =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdMoreMedium))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdMoreMedium))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        var allTracks = await TrackService.GetAllUserTracks();

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

app.MapPost(
    "/track/more",
    async () =>
    {
        if (!await PlaylistHelper.CheckValidityPlaylist(Costants.PlaylistIdMore))
        {
            return Results.BadRequest("Playlist not found");
        }

        if (!await PlaylistHelper.CheckIsEmptyPlaylist(Costants.PlaylistIdMore))
        {
            return Results.BadRequest("Playlist is not empty, please clear it and retry");
        }

        var allTracks = await TrackService.GetAllUserTracks();

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

app.MapPost(
    "/track/artist",
    async (string artistId, IdArtistPlaylistsBody idArtistPlaylistsBody) =>
    {
        // check if params are not empty
        if (
            artistId == string.Empty
            || idArtistPlaylistsBody.Less == string.Empty
            || idArtistPlaylistsBody.LessMedium == string.Empty
            || idArtistPlaylistsBody.MoreMedium == string.Empty
            || idArtistPlaylistsBody.More == string.Empty
        )
        {
            return Results.BadRequest("Invalid query params request");
        }

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

        var allTracksArtist = await TrackService.GetAllUserTracks(artistId);

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
