namespace tracksByPopularity.middlewares;

public class RedirectHomeMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // redirect to swagger if root
        if (context.Request.Path.Value == "/")
        {
            context.Response.Redirect("/swagger");
            return;
        }

        await next(context);
    }
}
