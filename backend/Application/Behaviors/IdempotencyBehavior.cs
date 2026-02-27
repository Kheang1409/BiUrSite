using Backend.Application.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Backend.Application.Behavior;

public interface IIdempotentCommand
{
    string IdempotencyKey { get; }
}

public sealed class IdempotencyBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IIdempotencyStore _store;
    private readonly ILogger<IdempotencyBehavior<TRequest, TResponse>> _logger;
    private static readonly TimeSpan DefaultTtl = TimeSpan.FromMinutes(10);

    public IdempotencyBehavior(IIdempotencyStore store, ILogger<IdempotencyBehavior<TRequest, TResponse>> logger)
    {
        _store = store;
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not IIdempotentCommand idempotentCommand)
        {
            return await next();
        }

        var key = $"{typeof(TRequest).Name}:{idempotentCommand.IdempotencyKey}";

        var (exists, cachedResponse) = await _store.TryGetAsync(key, cancellationToken);
        if (exists && cachedResponse is TResponse response)
        {
            _logger.LogWarning(
                "Duplicate request detected for {RequestType} with key {IdempotencyKey}",
                typeof(TRequest).Name,
                idempotentCommand.IdempotencyKey);
            return response;
        }

        var acquired = await _store.TryMarkInProgressAsync(key, DefaultTtl, cancellationToken);
        if (!acquired)
        {
            await Task.Delay(100, cancellationToken);
            var (retryExists, retryResponse) = await _store.TryGetAsync(key, cancellationToken);
            if (retryExists && retryResponse is TResponse retryResult)
            {
                return retryResult;
            }
        }

        try
        {
            var result = await next();
            await _store.SetAsync(key, result, DefaultTtl, cancellationToken);
            return result;
        }
        catch
        {
            await _store.RemoveAsync(key, cancellationToken);
            throw;
        }
    }
}
