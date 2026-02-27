using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Backend.API.Middleware;

public class RequestDiagnosticMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestDiagnosticMiddleware> _logger;

    public RequestDiagnosticMiddleware(RequestDelegate next, ILogger<RequestDiagnosticMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;
        var isInteresting = path.Contains("/notificationHub/negotiate", StringComparison.OrdinalIgnoreCase)
                            || path.Equals("/graphql", StringComparison.OrdinalIgnoreCase);

        if (!isInteresting)
        {
            await _next(context);
            return;
        }

        var requestInfo = new Dictionary<string, object>
        {
            ["method"] = context.Request.Method,
            ["path"] = path,
            ["isHttps"] = context.Request.IsHttps,
            ["queryString"] = context.Request.QueryString.Value ?? string.Empty,
            ["hasAuthorizationHeader"] = context.Request.Headers.ContainsKey("Authorization")
        };

        var headers = new Dictionary<string, string>();
        foreach (var h in context.Request.Headers)
        {
            if (string.Equals(h.Key, "Authorization", StringComparison.OrdinalIgnoreCase))
            {
                var enableTracing = string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1");
                headers[h.Key] = enableTracing ? h.Value.ToString() : "(present)";
                continue;
            }
            if (string.Equals(h.Key, "Cookie", StringComparison.OrdinalIgnoreCase)) continue;
            headers[h.Key] = h.Value.ToString();
        }

        requestInfo["headers"] = headers;

        string? bodyText = null;
        if (context.Request.ContentLength > 0 && context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();
            using var reader = new StreamReader(context.Request.Body, leaveOpen: true);
            bodyText = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;
            if (bodyText?.Length > 10_000)
            {
                bodyText = bodyText.Substring(0, 10_000) + "...";
            }
            if (bodyText is not null)
            {
                requestInfo["body"] = bodyText;
            }
        }

        var enableTracingGlobal = string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1");
        string? responseBodyText = null;
        if (enableTracingGlobal)
        {
            var originalBody = context.Response.Body;
            await using var memStream = new MemoryStream();
            context.Response.Body = memStream;
            await _next(context);
            memStream.Position = 0;
            using var reader = new StreamReader(memStream, leaveOpen: true);
            responseBodyText = await reader.ReadToEndAsync();
            memStream.Position = 0;
            await memStream.CopyToAsync(originalBody);
            context.Response.Body = originalBody;
        }
        else
        {
            await _next(context);
        }

        var status = context.Response.StatusCode;
        if (status >= 400)
        {
            var user = context.User?.Identity?.IsAuthenticated == true ? context.User?.Identity?.Name ?? "(unknown)" : "(unauthenticated)";
            var info = new Dictionary<string, object>(requestInfo)
            {
                ["statusCode"] = status,
                ["user"] = user
            };

            if (!string.IsNullOrEmpty(responseBodyText))
            {
                try
                {
                    info["responseBody"] = JsonSerializer.Deserialize<JsonElement>(responseBodyText);
                }
                catch
                {
                    info["responseBodyText"] = responseBodyText.Length > 2000 ? responseBodyText.Substring(0, 2000) + "..." : responseBodyText;
                }
            }

            try
            {
                var enableTracing = string.Equals(Environment.GetEnvironmentVariable("ENABLE_REQUEST_TRACING"), "1");
                if (enableTracing && context.Request.Headers.TryGetValue("Authorization", out var authValues))
                {
                    var auth = authValues.ToString();
                    info["authorization"] = auth;
                    if (auth.StartsWith("Bearer "))
                    {
                        var token = auth.Substring("Bearer ".Length).Trim();
                        var parts = token.Split('.');
                        if (parts.Length >= 2)
                        {
                            string payload = parts[1];
                            payload = payload.Replace('-', '+').Replace('_', '/');
                            switch (payload.Length % 4)
                            {
                                case 2: payload += "=="; break;
                                case 3: payload += "="; break;
                            }
                            var bytes = Convert.FromBase64String(payload);
                            var json = System.Text.Encoding.UTF8.GetString(bytes);
                            info["jwtPayload"] = JsonSerializer.Deserialize<JsonElement>(json);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                info["jwtDecodeError"] = ex.Message;
            }
            _logger.LogWarning("RequestDiagnostic: {Info}", JsonSerializer.Serialize(info));
        }
        else
        {
            _logger.LogDebug("RequestDiagnostic ok: {Method} {Path} => {Status}", context.Request.Method, path, status);
        }
    }
}
