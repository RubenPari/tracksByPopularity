using System.Security.Claims;
using tracksByPopularity.services;
using tracksByPopularity.utils;

namespace tracksByPopularity.middlewares;

public class ApiKeyAuthMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context, ApiKeyService apiKeyService)
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

        // Check if API key is provided in the header
        if (!context.Request.Headers.TryGetValue("X-API-Key", out var apiKeyValue))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("API key is required");
            return;
        }

        var apiKey = await apiKeyService.ValidateApiKeyAsync(apiKeyValue!);

        if (apiKey == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Invalid or expired API key");
            return;
        }

        // Create a ClaimsPrincipal for the authenticated user
        var claims = new List<Claim> { new(ClaimTypes.NameIdentifier, apiKey.UserId) };

        var identity = new ClaimsIdentity(claims, "ApiKey");
        context.User = new ClaimsPrincipal(identity);

        await next(context);
    }
}
