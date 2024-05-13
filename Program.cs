using dotenv.net;
using SpotifyAPI.Web;

var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

DotEnv.Load();

const int tracksLessPopularity = 33;
const int tracksMediumPopularity = 66;
SpotifyClient spotifyClient = null!;

var config = SpotifyClientConfig.CreateDefault();

// ---------- AUTHENTICATION ----------

app.MapGet("/auth/login", () =>
{
    var request = new LoginRequest(
        new Uri(Environment.GetEnvironmentVariable("REDIRECT_URI")!),
        Environment.GetEnvironmentVariable("CLIENT_ID")!,
        LoginRequest.ResponseType.Code)
    {
        Scope = new List<string>
        {
            Scopes.UserReadEmail,
            Scopes.UserReadPrivate,
            Scopes.UserLibraryRead,
            Scopes.UserLibraryModify,
            Scopes.PlaylistModifyPrivate,
            Scopes.PlaylistModifyPublic
        }
    };

    var uri = request.ToUri();

    return Results.Redirect(uri.ToString());
});

app.MapGet("/auth/callback", async (string code) =>
{
    var response = await new OAuthClient().RequestToken(
        new AuthorizationCodeTokenRequest(
            Environment.GetEnvironmentVariable("CLIENT_ID")!,
            Environment.GetEnvironmentVariable("CLIENT_SECRET")!,
            code,
            new Uri(Environment.GetEnvironmentVariable("REDIRECT_URI")!)
        )
    );

    spotifyClient = new SpotifyClient(config.WithToken(response.AccessToken));

    var user = await spotifyClient.UserProfile.Current();

    return user.Id == string.Empty
        ? Results.BadRequest("Login failed, retry")
        : Results.Ok("Successfully authenticated!");
});

// ---------- TRACK ----------

app.MapPost("/track/less", async () =>
{
    var firstPageTracks = await spotifyClient.Library.GetTracks();
    var allTracks = await spotifyClient.PaginateAll(firstPageTracks);

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity is > 0 and <= tracksLessPopularity)
        .ToList();

    // add the tracks to a playlist
    var playlist = await spotifyClient.Playlists.Get(Environment.GetEnvironmentVariable("PLAYLIST_ID_LESS")!);

    if (playlist.Id is null)
    {
        return Results.BadRequest("Playlist not found");
    }

    var trackPopularityCount = trackWithPopularity.Count;
    
    for (var i = 0; i < trackPopularityCount; i += 100)
    {
        var tracksToAdd = trackWithPopularity
            .Skip(i)
            .Take(100)
            .Select(track => track.Track.Uri)
            .ToList();

        var added = await spotifyClient.Playlists.AddItems(
            playlist.Id!,
            new PlaylistAddItemsRequest(tracksToAdd));

        if (added.SnapshotId == string.Empty)
        {
            return Results.BadRequest("Failed to add tracks to playlist");
        }
    }
    
    return Results.Ok("Tracks added to playlist");
});

app.MapPost("/track/medium", async () =>
{
    var firstPageTracks = await spotifyClient.Library.GetTracks();
    var allTracks = await spotifyClient.PaginateAll(firstPageTracks);

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity is > tracksLessPopularity and <= tracksMediumPopularity)
        .ToList();

    // add the tracks to a playlist
    var playlist = await spotifyClient.Playlists.Get(Environment.GetEnvironmentVariable("PLAYLIST_ID_MEDIUM")!);

    if (playlist.Id is null)
    {
        return Results.BadRequest("Playlist not found");
    }

    var trackPopularityCount = trackWithPopularity.Count;
    
    for (var i = 0; i < trackPopularityCount; i += 100)
    {
        var tracksToAdd = trackWithPopularity
            .Skip(i)
            .Take(100)
            .Select(track => track.Track.Uri)
            .ToList();

        var added = await spotifyClient.Playlists.AddItems(
            playlist.Id!,
            new PlaylistAddItemsRequest(tracksToAdd));

        if (added.SnapshotId == string.Empty)
        {
            return Results.BadRequest("Failed to add tracks to playlist");
        }
    }
    
    return Results.Ok("Tracks added to playlist");
});

app.MapPost("/track/more", async () =>
{
    var firstPageTracks = await spotifyClient.Library.GetTracks();
    var allTracks = await spotifyClient.PaginateAll(firstPageTracks);

    var trackWithPopularity = allTracks
        .Where(track => track.Track.Popularity > tracksMediumPopularity)
        .ToList();

    // add the tracks to a playlist
    var playlist = await spotifyClient.Playlists.Get(Environment.GetEnvironmentVariable("PLAYLIST_ID_MORE")!);

    if (playlist.Id is null)
    {
        return Results.BadRequest("Playlist not found");
    }

    var trackPopularityCount = trackWithPopularity.Count;
    
    for (var i = 0; i < trackPopularityCount; i += 100)
    {
        var tracksToAdd = trackWithPopularity
            .Skip(i)
            .Take(100)
            .Select(track => track.Track.Uri)
            .ToList();

        var added = await spotifyClient.Playlists.AddItems(
            playlist.Id!,
            new PlaylistAddItemsRequest(tracksToAdd));

        if (added.SnapshotId == string.Empty)
        {
            return Results.BadRequest("Failed to add tracks to playlist");
        }
    }
    
    return Results.Ok("Tracks added to playlist");
});

app.Run();