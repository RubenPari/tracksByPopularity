using tracksByPopularity.utils;

namespace tracksByPopularity.middlewares;

public class CheckAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // exclude swagger from auth check
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // exclude auth paths from auth check
        if (Constants.AuthPaths.Contains(context.Request.Path.Value))
        {
            await next(context);
            return;
        }

        if (Client.Spotify == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(context);
    }
}
