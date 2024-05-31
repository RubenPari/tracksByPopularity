namespace tracksByPopularity.middlewares;

public class CheckAuthMiddleware(RequestDelegate next)
{
    private static readonly string[] AuthPaths = ["/auth/login", "/auth/callback", "auth/logout", "/swagger"];

    public async Task InvokeAsync(HttpContext context)
    {
        if (AuthPaths.Contains(context.Request.Path))
        {
            await next(context);
            return;
        }

        var userProfile = await Client.Spotify.UserProfile.Current();

        if (string.IsNullOrEmpty(userProfile.Id))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(context);
    }
}