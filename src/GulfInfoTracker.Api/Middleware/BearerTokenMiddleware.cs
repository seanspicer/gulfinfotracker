namespace GulfInfoTracker.Api.Middleware;

public class BearerTokenMiddleware(RequestDelegate next, IConfiguration config, ILogger<BearerTokenMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext ctx)
    {
        var expected = config["Api:BearerToken"];

        if (string.IsNullOrWhiteSpace(expected))
        {
            logger.LogWarning("Api:BearerToken is not configured — all API requests are unauthenticated.");
            await next(ctx);
            return;
        }

        var auth = ctx.Request.Headers.Authorization.ToString();
        if (!auth.StartsWith("Bearer ") || auth["Bearer ".Length..] != expected)
        {
            ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
            ctx.Response.Headers.WWWAuthenticate = "Bearer";
            await ctx.Response.WriteAsync("Unauthorized");
            return;
        }

        await next(ctx);
    }
}
