using tracksByPopularity.controllers;

namespace tracksByPopularity.routes;

public static class Routes
{
    public static void MapRoutes(WebApplication app)
    {
        // ####### /AUTH #######

        var authRoutes = app.MapGroup("/auth");

        authRoutes.MapGet("/login", AuthController.Login);

        authRoutes.MapGet("/callback", AuthController.Callback);

        authRoutes.MapPost("/logout", AuthController.Logout);

        authRoutes.MapPost("/logout-all", AuthController.LogoutAll);

        // API Key management endpoints
        authRoutes.MapPost("/api-key", ApiKeyController.GenerateApiKey);

        authRoutes.MapGet("/api-keys", ApiKeyController.ListApiKeys);

        authRoutes.MapDelete("/api-key/{keyId}", ApiKeyController.RevokeApiKey);

        // ####### /TRACK #######

        var trackRoutes = app.MapGroup("/track");

        trackRoutes.MapPost("/less", TrackController.Less);

        trackRoutes.MapPost("/less-medium", TrackController.LessMedium);

        trackRoutes.MapPost("/medium", TrackController.Medium);

        trackRoutes.MapPost("/more-medium", TrackController.MoreMedium);

        trackRoutes.MapPost("/more", TrackController.More);

        trackRoutes.MapPost("/artist", TrackController.Artist);

        // TODO: move following code to regroup-musics project
        // ####### /PLAYLIST #######

        var playlistRoutes = app.MapGroup("/playlist");

        playlistRoutes.MapPost(
            "/create-playlist-track-minor",
            PlaylistController.CreatePlaylistTrackMinor
        );
    }
}
