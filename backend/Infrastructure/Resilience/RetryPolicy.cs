using Microsoft.Extensions.Logging;

namespace Backend.Infrastructure.Resilience;

public static class RetryPolicy
{
    public static async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        int maxRetries = 3,
        int baseDelayMs = 100,
        ILogger? logger = null,
        string operationName = "Operation",
        CancellationToken cancellationToken = default)
    {
        var attempt = 0;
        var exceptions = new List<Exception>();

        while (true)
        {
            try
            {
                attempt++;
                return await operation();
            }
            catch (Exception ex) when (IsTransient(ex) && attempt <= maxRetries)
            {
                exceptions.Add(ex);
                var delay = CalculateDelay(attempt, baseDelayMs);
                
                logger?.LogWarning(
                    ex,
                    "{OperationName} failed on attempt {Attempt}/{MaxRetries}. Retrying in {DelayMs}ms.",
                    operationName,
                    attempt,
                    maxRetries,
                    delay);

                await Task.Delay(delay, cancellationToken);
            }
        }
    }

    public static async Task ExecuteWithRetryAsync(
        Func<Task> operation,
        int maxRetries = 3,
        int baseDelayMs = 100,
        ILogger? logger = null,
        string operationName = "Operation",
        CancellationToken cancellationToken = default)
    {
        await ExecuteWithRetryAsync(
            async () =>
            {
                await operation();
                return true;
            },
            maxRetries,
            baseDelayMs,
            logger,
            operationName,
            cancellationToken);
    }

    private static bool IsTransient(Exception ex)
    {
        if (ex is MongoDB.Driver.MongoException mongoEx)
        {
            return mongoEx is MongoDB.Driver.MongoConnectionException
                || mongoEx is MongoDB.Driver.MongoWaitQueueFullException
                || ex.Message.Contains("timed out", StringComparison.OrdinalIgnoreCase);
        }

        if (ex is TimeoutException || ex is TaskCanceledException)
        {
            return true;
        }

        if (ex is HttpRequestException)
        {
            return true;
        }

        return false;
    }

    private static int CalculateDelay(int attempt, int baseDelayMs)
    {
        var exponentialDelay = baseDelayMs * (int)Math.Pow(2, attempt - 1);
        var jitter = Random.Shared.Next(0, exponentialDelay / 2);
        return Math.Min(exponentialDelay + jitter, 30000);
    }
}
