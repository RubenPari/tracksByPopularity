using tracksByPopularity.models;
using tracksByPopularity.utils;

namespace tracksByPopularity.middlewares;

public class JwtAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ITokenService tokenService)
    {
        // Exclude swagger from auth check
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            await next(context);
            return;
        }

        // Exclude auth paths from auth check
        if (Constants.AuthPaths.Contains(context.Request.Path.Value))
        {
            await next(context);
            return;
        }

        var authHeader = context.Request.Headers.Authorization.FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(
                "Unauthorized: Missing or invalid authorization header"
            );
            return;
        }

        var token = authHeader["Bearer ".Length..];
        var userToken = tokenService.ValidateToken(token);

        if (userToken == null)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("Unauthorized: Invalid token");
            return;
        }

        // Check if Spotify token has expired
        if (userToken.ExpiresAt < DateTime.UtcNow)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync(
                "Unauthorized: Spotify token has expired, please login again"
            );
            return;
        }

        // Add user claims to the context
        context.Items["UserId"] = userToken.UserId;
        context.Items["SpotifyToken"] = userToken.AccessToken;

        await next(context);
    }
}
