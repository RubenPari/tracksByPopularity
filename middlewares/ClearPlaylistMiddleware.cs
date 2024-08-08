using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;

namespace tracksByPopularity.middlewares;

public class ClearPlaylistMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Check if the path is sufficient to remove prefix
        if (context.Request.Path.Value!.Length < 6)
        {
            await next(context);
            return;
        }

        // Remove prefix "/track" from the path
        var path = context.Request.Path.Value[6..];

        if (path == "/top")
        {
            var timeRange = QueryParamHelper.GetTimeRangeQueryParam(context);

            RemoveAllTracksResponse cleared;

            switch (timeRange)
            {
                case TimeRangeEnum.ShortTerm:
                    cleared = await PlaylistService.RemoveAllTracks(Constants.PlaylistIdTopShort);
                    break;
                case TimeRangeEnum.MediumTerm:
                    cleared = await PlaylistService.RemoveAllTracks(Constants.PlaylistIdTopMedium);
                    break;
                case TimeRangeEnum.LongTerm:
                    cleared = await PlaylistService.RemoveAllTracks(Constants.PlaylistIdTopLong);
                    break;
                default:
                    cleared = RemoveAllTracksResponse.Success;
                    break;
            }

            var result = Results.Ok();

            switch (cleared)
            {
                case RemoveAllTracksResponse.Unauthorized:
                    result = Results.Problem(
                        detail: $"Unauthorized please login to {Constants.ClearSongsBaseUrl}/auth/login and retry",
                        statusCode: 401);
                    break;
                case RemoveAllTracksResponse.BadRequest:
                    result = Results.BadRequest("Something went wrong, please try again later");
                    break;
            }

            await result.ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}