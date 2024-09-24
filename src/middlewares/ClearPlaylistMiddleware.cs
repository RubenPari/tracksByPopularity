using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;
using tracksByPopularity.src.helpers;

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

            var cleared = timeRange switch
            {
                TimeRangeEnum.ShortTerm => await PlaylistService.RemoveAllTracks(
                    Constants.PlaylistIdTopShort
                ),
                TimeRangeEnum.MediumTerm => await PlaylistService.RemoveAllTracks(
                    Constants.PlaylistIdTopMedium
                ),
                TimeRangeEnum.LongTerm => await PlaylistService.RemoveAllTracks(
                    Constants.PlaylistIdTopLong
                ),
                _ => RemoveAllTracksResponse.Success,
            };

            var result = Results.Ok();

            switch (cleared)
            {
                case RemoveAllTracksResponse.Unauthorized:
                    result = Results.Problem(
                        detail: $"Unauthorized please login to {Constants.ClearSongsBaseUrl}/auth/login and retry",
                        statusCode: 401
                    );
                    break;
                case RemoveAllTracksResponse.BadRequest:
                    result = Results.BadRequest("Something went wrong, please try again later");
                    break;
                case RemoveAllTracksResponse.Success:
                    break;
                default:
                    throw new Exception("Invalid response");
            }

            await result.ExecuteAsync(context);
            return;
        }

        await next(context);
    }
}
