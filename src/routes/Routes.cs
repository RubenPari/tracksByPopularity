﻿using tracksByPopularity.controllers;
using tracksByPopularity.services;

namespace tracksByPopularity.routes;

public static class Routes
{
    public static void MapRoutes(WebApplication app)
    {
        // ####### /AUTH #######

        var authRoutes = app.MapGroup("/auth");

        authRoutes.MapGet("/login", AuthController.Login);

        authRoutes.MapGet("/callback", async (string code, SpotifyAuthService authService) => await AuthController.Callback(code, authService));
        
        authRoutes.MapGet("/logout", AuthController.Logout);

        // ####### /TRACK #######

        var trackRoutes = app.MapGroup("/track");

        trackRoutes.MapPost("/top", TrackController.Top50);

        trackRoutes.MapPost("/less", TrackController.Less);

        trackRoutes.MapPost("/less-medium", TrackController.LessMedium);

        trackRoutes.MapPost("/more-medium", TrackController.MoreMedium);

        trackRoutes.MapPost("/more", TrackController.More);

        trackRoutes.MapPost("/artist", TrackController.Artist);

        // ####### /PLAYLIST #######

        var playlistRoutes = app.MapGroup("/playlist");

        playlistRoutes.MapPost(
            "/create-playlist-track-minor",
            PlaylistController.CreatePlaylistTrackMinor
        );
    }
}
