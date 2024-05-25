using dotenv.net;
using SpotifyAPI.Web;
using tracksByPopularity;
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

app.UseOpenApi();
app.UseSwaggerUi(config =>
{
    config.DocumentTitle = Costants.TitleApi;
    config.Path = "/swagger";
    config.DocumentPath = "/swagger/{documentName}/swagger.json";
    config.DocExpansion = "list";
});

DotEnv.Load();

app.MapGet("/auth/login", () =>
{
    var request = new LoginRequest(
        new Uri(Costants.RedirectUri),
        Costants.ClientId,
        LoginRequest.ResponseType.Code)
    {
        Scope = Costants.MyScopes
    };

    var uri = request.ToUri();

    return Results.Redirect(uri.ToString());
});

app.MapGet("/auth/callback", async (string code) =>
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
    Client.AccessToken = response.AccessToken;

    var user = await Client.Spotify.UserProfile.Current();

    return user.Id == string.Empty
        ? Results.BadRequest("Login failed, retry")
        : Results.Ok("Successfully authenticated!");
});

app.MapPost("/track/less", async () =>
{
    var deletedTracksPlaylist = await TrackService.RemoveTracksFromPlaylist(Costants.PlaylistIdLess);

    if (!deletedTracksPlaylist)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks();

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity > 0 && track.Track.Popularity <= Costants.TracksLessPopularity)
        .ToList();

    var playlist = await Client.Spotify.Playlists.Get(Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!);

    if (playlist.Id is null)
    {
        return Results.BadRequest("Playlist not found");
    }

    var added = await TrackService.AddTracksToPlaylist(trackWithPopularity, playlist.Id);

    return !added ? Results.BadRequest("Failed to add tracks to playlist") : Results.Ok("Tracks added to playlist");
});

app.MapPost("/track/medium", async () =>
{
    var deletedTracksPlaylist = await TrackService.RemoveTracksFromPlaylist(Costants.PlaylistIdMedium);

    if (!deletedTracksPlaylist)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks();

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity > Costants.TracksLessPopularity &&
                        track.Track.Popularity <= Costants.TracksMediumPopularity)
        .ToList();

    var playlist = await Client.Spotify.Playlists.Get(Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM")!);

    if (playlist.Id is null)
    {
        return Results.BadRequest("Playlist not found");
    }

    var added = await TrackService.AddTracksToPlaylist(trackWithPopularity, playlist.Id);

    return !added ? Results.BadRequest("Failed to add tracks to playlist") : Results.Ok("Tracks added to playlist");
});

app.MapPost("/track/more", async () =>
{
    var deletedTracksPlaylist = await TrackService.RemoveTracksFromPlaylist(Costants.PlaylistIdMore);

    if (!deletedTracksPlaylist)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks();

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity > Costants.TracksMediumPopularity)
        .ToList();

    var playlist = await Client.Spotify.Playlists.Get(Costants.PlaylistIdMore);

    if (playlist.Id is null)
    {
        return Results.BadRequest("Playlist not found");
    }

    var added = await TrackService.AddTracksToPlaylist(trackWithPopularity, playlist.Id);

    return !added ? Results.BadRequest("Failed to add tracks to playlist") : Results.Ok("Tracks added to playlist");
});

app.MapPost("/track/delete", async (int popularity) =>
{
    // check if popularity is valid
    if (popularity is < 0 or > 100)
    {
        return Results.BadRequest("Invalid popularity");
    }

    // get all tracks
    var allTracks = await TrackService.GetAllUserTracks();

    // filter tracks by popularity minor or equal to the input
    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity <= popularity)
        .ToList();

    // remove tracks from user library
    var deletedTracks = await TrackService.RemoveUserTracks(trackWithPopularity);

    return !deletedTracks ? Results.BadRequest("Failed to delete tracks") : Results.Ok("Tracks deleted");
});

app.MapPost("/track/artist", async (string artistId, string popularType, IdPlaylists idPlaylists) =>
{
    if (artistId == string.Empty || popularType == string.Empty)
    {
        return Results.BadRequest("Invalid query params request");
    }

    var deletedTracksPlaylistLess = await TrackService.RemoveTracksFromPlaylist(idPlaylists.IdLess);
    var deletedTracksPlaylistMedium = await TrackService.RemoveTracksFromPlaylist(idPlaylists.IdMedium);
    var deletedTracksPlaylistMore = await TrackService.RemoveTracksFromPlaylist(idPlaylists.IdMore);

    if (!deletedTracksPlaylistLess || !deletedTracksPlaylistMedium || !deletedTracksPlaylistMore)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks(artistId);

    var trackWithPopularity = new List<SavedTrack>();

    switch (popularType)
    {
        case "less":
            trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity > 0 && track.Track.Popularity <= Costants.TracksLessPopularity)
                .ToList();

            break;
        case "medium":
            trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity > Costants.TracksLessPopularity &&
                                track.Track.Popularity <= Costants.TracksMediumPopularity)
                .ToList();

            break;
        case "more":
            trackWithPopularity = allTracks
                .Where(track => track.Track.Popularity > Costants.TracksMediumPopularity)
                .ToList();

            break;
    }

    var addedLess = await AddTracksToPlaylist(idPlaylists.IdLess, trackWithPopularity);
    var addedMedium = await AddTracksToPlaylist(idPlaylists.IdMedium, trackWithPopularity);
    var addedMore = await AddTracksToPlaylist(idPlaylists.IdMore, trackWithPopularity);

    if (!addedLess || !addedMedium || !addedMore)
    {
        return Results.BadRequest("Failed to add tracks to playlist");
    }

    return Results.Ok("Tracks added to playlist");
});

app.Run();
return;

static async Task<bool> AddTracksToPlaylist(string artistPlaylistId, IList<SavedTrack> trackWithPopularity)
{
    var playlist = await Client.Spotify.Playlists.Get(artistPlaylistId);

    if (playlist.Id is null)
    {
        return false;
    }

    return await TrackService.AddTracksToPlaylist(trackWithPopularity, playlist.Id);
}