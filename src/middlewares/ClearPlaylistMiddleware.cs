using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.middlewares;

public class ClearPlaylistMiddleware(
    RequestDelegate next,
    IHttpClientFactory? httpClientFactory = null
)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.Value!.Contains("/auth"))
        {
            await next(context);
            return;
        }

        if (!await IsAuthenticatedWithClearSongsService())
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized, login to clear-songs service");
            return;
        }

        var path = context.Request.Path.Value!;

        if (path.Contains("/top"))
        {
            await HandleTopPath(context);
            return;
        }

        var playlistId = GetPlaylistIdFromPath(path);

        if (playlistId != null)
        {
            await HandlePlaylistClear(context, playlistId);
        }

        await next(context);
    }

    /// <summary>
    /// Checks if the user is authenticated with the ClearSongsService.
    /// </summary>
    /// <returns>
    /// A boolean indicating whether the user is authenticated or not.
    /// </returns>
    private async Task<bool> IsAuthenticatedWithClearSongsService()
    {
        var http = httpClientFactory?.CreateClient() ?? new HttpClient();

        try
        {
            var response = await http.GetAsync("http://localhost:3000/auth/is-auth");
            return response.IsSuccessStatusCode;
        }
        finally
        {
            if (httpClientFactory == null)
            {
                http.Dispose();
            }
        }
    }

    // Handles the top path by getting the time range from the query parameter,
    // removing all tracks from the corresponding playlist, and returning the result.
    // Handles the top path by getting the time range from the query parameter,
    // removing all tracks from the corresponding playlist, and returning the result.
    //
    // Parameters:
    //   context (HttpContext): The HTTP context of the request.
    //
    // Returns:
    //   Task: A task representing the asynchronous operation.
    private static async Task HandleTopPath(HttpContext context)
    {
        var timeRange = QueryParamHelper.GetTimeRangeQueryParam(context);

        var playlistId = GetPlaylistIdForTimeRange(timeRange);

        var cleared = await PlaylistService.RemoveAllTracks(playlistId);

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
            _ => throw new Exception("Invalid response"),
        };

        await result.ExecuteAsync(context);
    }

    /// <summary>
    /// Retrieves the playlist ID corresponding to the given time range.
    ///
    /// Parameters:
    ///     timeRange (TimeRangeEnum): The time range for which to retrieve the playlist ID.
    ///
    /// Returns:
    ///     string: The playlist ID corresponding to the given time range.
    ///
    /// </summary>
    private static string GetPlaylistIdForTimeRange(TimeRangeEnum timeRange) =>
        timeRange switch
        {
            TimeRangeEnum.ShortTerm => Constants.PlaylistIdTopShort,
            TimeRangeEnum.MediumTerm => Constants.PlaylistIdTopMedium,
            TimeRangeEnum.LongTerm => Constants.PlaylistIdTopLong,
            TimeRangeEnum.NotValid => throw new ArgumentOutOfRangeException(
                nameof(timeRange),
                timeRange,
                null
            ),
            _ => throw new ArgumentOutOfRangeException(nameof(timeRange), timeRange, null),
        };

    /// <summary>
    /// Retrieves the playlist ID corresponding to the given path.
    ///
    /// Parameters:
    ///     path (string): The path from which to retrieve the playlist ID.
    ///
    /// Returns:
    ///     string?: The playlist ID corresponding to the given path, or null if no match is found.
    /// </summary>
    private static string? GetPlaylistIdFromPath(string path)
    {
        // remove the /track prefix from path
        path = path.Remove(0, 6);

        return path switch
        {
            "/less" => Constants.PlaylistIdLess,
            "/less-medium" => Constants.PlaylistIdLessMedium,
            "/more-medium" => Constants.PlaylistIdMoreMedium,
            "/more" => Constants.PlaylistIdMore,
            _ => null,
        };
    }

    /// <summary>
    /// Handles the clearing of a playlist by removing all tracks from the specified playlist ID.
    ///
    /// Parameters:
    ///     context (HttpContext): The HTTP context of the request.
    ///     playlistId (string): The ID of the playlist to clear.
    ///
    /// Returns:
    ///     Task: A task representing the asynchronous operation.
    /// </summary>
    private static async Task HandlePlaylistClear(HttpContext context, string playlistId)
    {
        var deleted = await PlaylistService.RemoveAllTracks(playlistId);

        if (deleted != RemoveAllTracksResponse.Success)
        {
            context.Response.StatusCode = 400;
            await context.Response.WriteAsync("Something went wrong, please try again later");
        }
    }
}