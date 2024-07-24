using tracksByPopularity.helpers;
using tracksByPopularity.models;
using tracksByPopularity.services;

namespace tracksByPopularity.middlewares;

public class ClearPlaylistMiddleware(RequestDelegate next)
{
    private static readonly string[] AuthPaths = ["/auth/login", "/auth/callback", "/auth/logout"];

    public async Task<IResult> InvokeAsync(HttpContext context)
    {
        // exclude swagger from auth check
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return Results.Ok();
        }

        // exclude auth paths from auth check
        if (AuthPaths.Contains(context.Request.Path.Value))
        {
            await next(context);
            return Results.Ok();
        }

        // remove from context.Request.Path.Value the "/track" prefix
        var path = context.Request.Path.Value![6..];
        var cleared = false;

        switch (path)
        {
            case "/top":
                var timeRange = QueryParamHelper.GetTimeRangeQueryParam(context);

                cleared = timeRange switch
                {
                    TimeRangeEnum.ShortTerm => await PlaylistService.RemoveAllTracks(Constants.PlaylistIdTopShort),
                    TimeRangeEnum.MediumTerm => await PlaylistService.RemoveAllTracks(Constants.PlaylistIdTopMedium),
                    TimeRangeEnum.LongTerm => await PlaylistService.RemoveAllTracks(Constants.PlaylistIdTopLong),
                    _ => cleared
                };

                if (!cleared)
                {
                    return Results.BadRequest("Failed to clear playlist");
                }

                await next(context);
                break;
            default:
                await next(context);
                return Results.Ok();
        }

        await next(context);
        return Results.Ok();
    }
}