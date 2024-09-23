using tracksByPopularity.src.controllers;

namespace tracksByPopularity.src.routes;

public class Routes
{
    public static void MapRoutes(WebApplication app)
    {
        // ####### /AUTH #######

        var authRoutes = app.MapGroup("/auth");

        authRoutes.MapGet("/login", AuthController.Login);

        authRoutes.MapGet("/callback", AuthController.Callback);

        authRoutes.MapGet("/logout", AuthController.Logout);

        // ####### /TRACK #######

        var trackRoutes = app.MapGroup("/track");

        trackRoutes.MapPost("/top", TrackController.Top);

        trackRoutes.MapPost("/less", TrackController.Less);

        trackRoutes.MapPost("/less-medium", TrackController.LessMedium);

        trackRoutes.MapPost("/more-medium", TrackController.MoreMedium);

        trackRoutes.MapPost("/more", TrackController.More);

        trackRoutes.MapPost("/artist", TrackController.Artist);
    }
}
