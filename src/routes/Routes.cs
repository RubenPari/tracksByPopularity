using tracksByPopularity.controllers;

namespace tracksByPopularity.routes;

public abstract class Routes
{
    public static void MapRoutes(WebApplication app)
    {
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
