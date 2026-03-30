using System.Net;
using System.Text.Json;
using StackExchange.Redis;
using SpotifyAPI.Web;

namespace tracksByPopularity.Presentation.Middlewares;

public class ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = HttpStatusCode.InternalServerError;
        var message = "An unexpected error occurred.";

        switch (exception)
        {
            case UnauthorizedAccessException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Unauthorized access attempt. Please check your Spotify session.";
                logger.LogWarning(exception, "Unauthorized access attempt");
                break;
            case APIUnauthorizedException:
                statusCode = HttpStatusCode.Unauthorized;
                message = "Spotify session expired. Please log in again.";
                logger.LogWarning(exception, "Spotify API unauthorized");
                break;
            case APIException apiEx:
                statusCode = HttpStatusCode.BadGateway;
                message = $"Spotify API error: {apiEx.Message}";
                logger.LogError(apiEx, "Spotify API error");
                break;
            case DomainException domainEx:
                statusCode = HttpStatusCode.BadRequest;
                message = domainEx.Message;
                logger.LogWarning(domainEx, "Domain error");
                break;
            case ArgumentException argEx:
                statusCode = HttpStatusCode.BadRequest;
                message = argEx.Message;
                logger.LogWarning(argEx, "Bad request");
                break;
            case RedisConnectionException:
                statusCode = HttpStatusCode.ServiceUnavailable;
                message = "Cache service temporarily unavailable. Please try again.";
                logger.LogError(exception, "Redis connection error");
                break;
            case RedisTimeoutException:
                statusCode = HttpStatusCode.GatewayTimeout;
                message = "Cache service timed out. Please try again.";
                logger.LogError(exception, "Redis timeout error");
                break;
            default:
                logger.LogError(exception, "Unhandled error occurred.");
                break;
        }

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = ApiResponse.Fail(message);

        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var jsonResponse = JsonSerializer.Serialize(response, options);

        await context.Response.WriteAsync(jsonResponse);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static void UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
