using dotenv.net;
using SpotifyAPI.Web;
using tracksByPopularity;
using tracksByPopularity.services;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

DotEnv.Load();

// ---------- CONSTANTS ----------

const int tracksLessPopularity = 33;
const int tracksMediumPopularity = 66;

List<string> scopes =
[
    Scopes.UserReadEmail,
    Scopes.UserReadPrivate,
    Scopes.UserLibraryRead,
    Scopes.UserLibraryModify,
    Scopes.PlaylistModifyPrivate,
    Scopes.PlaylistModifyPublic
];

var clientId = Environment.GetEnvironmentVariable("CLIENT_ID")!;
var clientSecret = Environment.GetEnvironmentVariable("CLIENT_SECRET")!;
var redirectUri = Environment.GetEnvironmentVariable("REDIRECT_URI")!;

var playlistIdLess = Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!;
var playlistIdMedium = Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM")!;
var playlistIdMore = Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!;

var config = SpotifyClientConfig.CreateDefault();

// ---------- AUTHENTICATION ----------

app.MapGet("/auth/login", () =>
{
    var request = new LoginRequest(
        new Uri(redirectUri),
        clientId,
        LoginRequest.ResponseType.Code)
    {
        Scope = scopes
    };

    var uri = request.ToUri();

    return Results.Redirect(uri.ToString());
});

app.MapGet("/auth/callback", async (string code) =>
{
    var response = await new OAuthClient().RequestToken(
        new AuthorizationCodeTokenRequest(
            clientId,
            clientSecret,
            code,
            new Uri(redirectUri)
        )
    );

    Client.Spotify = new SpotifyClient(config.WithToken(response.AccessToken));
    Client.AccessToken = response.AccessToken;
    
    var user = await Client.Spotify.UserProfile.Current();

    return user.Id == string.Empty
        ? Results.BadRequest("Login failed, retry")
        : Results.Ok("Successfully authenticated!");
});

// ---------- TRACK ----------

app.MapPost("/track/less", async () =>
{
    var deletedTracksPlaylist = await TrackService.RemoveTracksFromPlaylist(playlistIdLess);

    if (!deletedTracksPlaylist)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks();

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity is > 0 and <= tracksLessPopularity)
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
    var deletedTracksPlaylist = await TrackService.RemoveTracksFromPlaylist(playlistIdMedium);

    if (!deletedTracksPlaylist)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks();

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity is > tracksLessPopularity and <= tracksMediumPopularity)
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
    var deletedTracksPlaylist = await TrackService.RemoveTracksFromPlaylist(playlistIdMore);

    if (!deletedTracksPlaylist)
    {
        return Results.BadRequest("Failed to delete tracks from playlist");
    }

    var allTracks = await TrackService.GetAllUserTracks();

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity is > tracksMediumPopularity)
        .ToList();

    var playlist = await Client.Spotify.Playlists.Get(Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!);

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

app.Run();