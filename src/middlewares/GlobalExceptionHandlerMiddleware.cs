using System.Net;
using System.Text.Json;

namespace tracksByPopularity.middlewares;

/// <summary>
/// Middleware for global exception handling across the application.
/// Catches unhandled exceptions and returns standardized error responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
    /// </summary>
    /// <param name="next">The next middleware in the pipeline.</param>
    /// <param name="logger">The logger instance for recording exceptions.</param>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger
    )
    {
        _next = next;
        _logger = logger;
    }

    /// <summary>
    /// Invokes the middleware to process the HTTP request and handle any exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// This method wraps the request pipeline execution in a try-catch block.
    /// Any unhandled exceptions are caught, logged, and converted to standardized
    /// JSON error responses before being returned to the client.
    /// </remarks>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    /// <summary>
    /// Handles exceptions by converting them to standardized JSON error responses.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that was caught.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    /// <remarks>
    /// Maps exception types to appropriate HTTP status codes:
    /// - <see cref="UnauthorizedAccessException"/> → 401 Unauthorized
    /// - <see cref="ArgumentException"/> → 400 Bad Request
    /// - All other exceptions → 500 Internal Server Error
    /// 
    /// The response format is:
    /// {
    ///   "success": false,
    ///   "error": "error message",
    ///   "errorCode": statusCode
    /// }
    /// </remarks>
    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var statusCode = exception switch
        {
            UnauthorizedAccessException => HttpStatusCode.Unauthorized,
            ArgumentException => HttpStatusCode.BadRequest,
            ArgumentNullException => HttpStatusCode.BadRequest,
            domain.exceptions.DomainException => HttpStatusCode.BadRequest,
            application.exceptions.PlaylistOperationException => HttpStatusCode.BadRequest,
            _ => HttpStatusCode.InternalServerError,
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)statusCode;

        var response = new
        {
            success = false,
            error = exception.Message,
            errorCode = (int)statusCode,
        };

        var json = JsonSerializer.Serialize(response);
        return context.Response.WriteAsync(json);
    }
}

