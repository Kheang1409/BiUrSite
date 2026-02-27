using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace Backend.Infrastructure.Persistence;

public sealed class TransactionScope : IAsyncDisposable
{
    private readonly IClientSessionHandle _session;
    private readonly ILogger? _logger;
    private bool _committed;

    private TransactionScope(IClientSessionHandle session, ILogger? logger)
    {
        _session = session;
        _logger = logger;
    }

    public static async Task<TransactionScope> BeginAsync(
        MongoDbContext context,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        var session = await context.StartSessionAsync(cancellationToken);
        session.StartTransaction();
        logger?.LogDebug("MongoDB transaction started");
        return new TransactionScope(session, logger);
    }

    public IClientSessionHandle Session => _session;

    public async Task CommitAsync(CancellationToken cancellationToken = default)
    {
        if (_committed)
            return;
            
        await _session.CommitTransactionAsync(cancellationToken);
        _committed = true;
        _logger?.LogDebug("MongoDB transaction committed");
    }

    public async Task AbortAsync(CancellationToken cancellationToken = default)
    {
        if (_committed)
            return;
            
        await _session.AbortTransactionAsync(cancellationToken);
        _logger?.LogDebug("MongoDB transaction aborted");
    }

    public async ValueTask DisposeAsync()
    {
        if (!_committed && _session.IsInTransaction)
        {
            try
            {
                await _session.AbortTransactionAsync();
                _logger?.LogWarning("MongoDB transaction was not committed and has been aborted on dispose");
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "Failed to abort MongoDB transaction on dispose");
            }
        }
        _session.Dispose();
    }
}

public static class TransactionExtensions
{
    public static async Task<T> ExecuteInTransactionAsync<T>(
        this MongoDbContext context,
        Func<IClientSessionHandle, Task<T>> action,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        await using var scope = await TransactionScope.BeginAsync(context, logger, cancellationToken);
        try
        {
            var result = await action(scope.Session);
            await scope.CommitAsync(cancellationToken);
            return result;
        }
        catch
        {
            await scope.AbortAsync(cancellationToken);
            throw;
        }
    }

    public static async Task ExecuteInTransactionAsync(
        this MongoDbContext context,
        Func<IClientSessionHandle, Task> action,
        ILogger? logger = null,
        CancellationToken cancellationToken = default)
    {
        await using var scope = await TransactionScope.BeginAsync(context, logger, cancellationToken);
        try
        {
            await action(scope.Session);
            await scope.CommitAsync(cancellationToken);
        }
        catch
        {
            await scope.AbortAsync(cancellationToken);
            throw;
        }
    }
}
