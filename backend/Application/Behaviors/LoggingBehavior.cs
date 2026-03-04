using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Behavior;

public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var requestId = Guid.NewGuid();

        _logger.LogDebug(
            "Handling {RequestName} ({RequestId})",
            requestName,
            requestId);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next();
            stopwatch.Stop();

            _logger.LogDebug(
                "Handled {RequestName} ({RequestId}) in {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Error handling {RequestName} ({RequestId}) after {ElapsedMilliseconds}ms",
                requestName,
                requestId,
                stopwatch.ElapsedMilliseconds);

            throw;
        }
    }
}
