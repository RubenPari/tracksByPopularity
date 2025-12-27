using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;

namespace tracksByPopularity.middlewares;

/// <summary>
/// Middleware that automatically clears playlists before adding new tracks.
/// Handles authentication, routing, and playlist clearing orchestration.
/// </summary>
public class ClearPlaylistMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IAuthenticationService _authenticationService;
    private readonly IPlaylistRoutingService _routingService;
    private readonly IPlaylistClearingService _clearingService;
    private readonly ILogger<ClearPlaylistMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="ClearPlaylistMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="authenticationService">Service for checking authentication.</param>
    /// <param name="routingService">Service for routing and playlist ID resolution.</param>
    /// <param name="clearingService">Service for clearing playlists.</param>
    /// <param name="logger">Logger instance for recording middleware activities.</param>
    public ClearPlaylistMiddleware(
        RequestDelegate next,
        IAuthenticationService authenticationService,
        IPlaylistRoutingService routingService,
        IPlaylistClearingService clearingService,
        ILogger<ClearPlaylistMiddleware> logger
    )
    {
        _next = next;
        _authenticationService = authenticationService;
        _routingService = routingService;
        _clearingService = clearingService;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // Skip authentication check for auth endpoints
        if (!_routingService.ShouldHandlePath(path))
        {
            await _next(context);
            return;
        }

        // Check authentication
        if (!await _authenticationService.IsAuthenticatedWithClearSongsServiceAsync())
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
            await HandleTopPath(context);
            return;
        }

        // Handle standard track paths
        var playlistId = _routingService.GetPlaylistIdFromPath(path);
        if (playlistId != null)
        {
            await HandlePlaylistClear(context, playlistId);
        }

        await _next(context);
    }

    /// <summary>
    /// Handles the top path by getting the time range from the query parameter,
    /// removing all tracks from the corresponding playlist, and returning the result.
    /// </summary>
    /// <param name="context">The HTTP context of the request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task HandleTopPath(HttpContext context)
    {
        var timeRange = QueryParamHelper.GetTimeRangeQueryParam(context);

        if (timeRange == TimeRangeEnum.NotValid)
        {
            _logger.LogWarning("Invalid time range parameter in request");
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Invalid time range parameter");
            return;
        }

        var playlistId = _routingService.GetPlaylistIdForTimeRange(timeRange);
        var cleared = await _clearingService.ClearPlaylistAsync(playlistId);

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
    private async Task HandlePlaylistClear(HttpContext context, string playlistId)
    {
        var result = await _clearingService.ClearPlaylistAsync(playlistId);

        if (result != RemoveAllTracksResponse.Success)
        {
            _logger.LogWarning("Failed to clear playlist {PlaylistId}. Response: {Response}", playlistId, result);
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Something went wrong, please try again later");
        }
    }
}
