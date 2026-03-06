using tracksByPopularity.Infrastructure.Helpers;
using tracksByPopularity.Domain.Enums;
using tracksByPopularity.Application.Services;

namespace tracksByPopularity.Presentation.Middlewares;

/// <summary>
/// Middleware that automatically clears playlists before adding new tracks.
/// Handles authentication, routing, and playlist clearing orchestration.
/// </summary>
public class ClearPlaylistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ClearPlaylistMiddleware> _logger;
    public ClearPlaylistMiddleware(
        RequestDelegate next,
        ILogger<ClearPlaylistMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(
        HttpContext context,
        IAuthenticationService authenticationService,
        IPlaylistRoutingService routingService,
        IPlaylistClearingService clearingService)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip middleware for health check and auth endpoints
        if (path.Equals("/health", StringComparison.OrdinalIgnoreCase) ||
            !routingService.ShouldHandlePath(path))
        {
            await _next(context);
            return;
        }

        // Check authentication
        if (!await authenticationService.IsAuthenticatedWithClearSongsServiceAsync())
        {
            _logger.LogWarning("Unauthorized access attempt to path: {Path}", path);
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized, login to clear-songs service");
            return;
        }

        // Handle top/artist paths
        if (path.Contains("/top", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("/artist", StringComparison.OrdinalIgnoreCase))
        {
            await HandleTopPath(context, routingService, clearingService);
            return;
        }

        // Handle standard track paths
        var playlistId = routingService.GetPlaylistIdFromPath(path);
        if (playlistId != null)
        {
            await HandlePlaylistClear(context, clearingService, playlistId);
        }

        await _next(context);
    }

    /// <summary>
    /// Handles the top path by getting the time range from the query parameter,
    /// removing all tracks from the corresponding playlist, and returning the result.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleTopPath(
        HttpContext context,
        IPlaylistRoutingService routingService,
        IPlaylistClearingService clearingService)
    {
        var timeRange = QueryParamHelper.GetTimeRangeQueryParam(context);

        if (timeRange == TimeRangeEnum.NotValid)
        {
            _logger.LogWarning("Invalid time range parameter in request");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid time range parameter");
            return;
        }

        var playlistId = routingService.GetPlaylistIdForTimeRange(timeRange);
        var cleared = await clearingService.ClearPlaylistAsync(playlistId);

        var result = cleared switch
        {
            RemoveAllTracksResponse.Unauthorized => Results.Problem(
                detail: "Unauthorized please login to http://localhost:3000/auth/login and retry",
                statusCode: 401
            ),
            RemoveAllTracksResponse.BadRequest => Results.BadRequest(
                "Something went wrong, please try again later"
            ),
            RemoveAllTracksResponse.Success => Results.Ok(),
            _ => throw new InvalidOperationException($"Invalid response: {cleared}"),
        };

        await result.ExecuteAsync(context);
    }

    /// <summary>
    /// Handles the clearing of a playlist by removing all tracks from the specified playlist ID.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <param name="playlistId">The ID of the playlist to clear.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandlePlaylistClear(
        HttpContext context,
        IPlaylistClearingService clearingService,
        string playlistId)
    {
        var result = await clearingService.ClearPlaylistAsync(playlistId);

        if (result != RemoveAllTracksResponse.Success)
        {
            _logger.LogWarning("Failed to clear playlist {PlaylistId}. Response: {Response}", playlistId, result);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Something went wrong, please try again later");
        }
    }
}
