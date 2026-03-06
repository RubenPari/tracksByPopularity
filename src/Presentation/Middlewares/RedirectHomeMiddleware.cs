namespace tracksByPopularity.Presentation.Middlewares;

public class RedirectHomeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
    }
}
