using System.Net;
using Backend.Application.Configuration;
using Backend.Application.Services;

namespace Backend.API.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IRateLimiter _rateLimiter;
    private readonly RateLimitOptions _options;

    public RateLimitingMiddleware(RequestDelegate next, IRateLimiter rateLimiter, Microsoft.Extensions.Options.IOptions<RateLimitOptions> options)
    {
        _next = next;
        _rateLimiter = rateLimiter;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Compose a key: per-IP or per-user if authenticated
        var identity = context.User?.Identity?.IsAuthenticated == true
            ? context.User.Identity!.Name ?? context.User.FindFirst("sub")?.Value ?? "anon"
            : context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var path = context.Request.Path.Value ?? "/";
        var key = $"{identity}:{path}";

        var allowed = await _rateLimiter.ShouldAllowAsync(key, _options.RequestLimit, TimeSpan.FromSeconds(_options.WindowSeconds), context.RequestAborted);
        if (!allowed)
        {
            context.Response.StatusCode = (int)HttpStatusCode.TooManyRequests;
            await context.Response.WriteAsync("Too Many Requests");
            return;
        }

        await _next(context);
    }
}
