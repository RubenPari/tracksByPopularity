namespace tracksByPopularity.middlewares;

public class RedirectHomeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        await next(context);
    }
}
