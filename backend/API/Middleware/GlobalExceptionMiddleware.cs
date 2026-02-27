using System.Diagnostics;
using System.Text.Json;

namespace Backend.API.Middleware;

public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly ExceptionHandlerContext _handlerContext;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _handlerContext = new ExceptionHandlerContext(env);
    }

    public async Task InvokeAsync(HttpContext context, ICorrelationIdAccessor correlationIdAccessor)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            var correlationId = correlationIdAccessor.CorrelationId ?? context.TraceIdentifier;
            
            _logger.LogError(
                ex,
                "Unhandled exception occurred. CorrelationId: {CorrelationId}",
                correlationId);
            
            var problem = _handlerContext.Handle(ex, context);

            problem.Type = $"{context.Request.Scheme}://{context.Request.Host}/api/errors/{ex.GetType().Name}";
            problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
            problem.Extensions["correlationId"] = correlationId;
            problem.Extensions["success"] = false;
            
            context.Response.ContentType = "application/problem+json";
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

            await context.Response.WriteAsync(JsonSerializer.Serialize(problem, options));
        }
    }
}