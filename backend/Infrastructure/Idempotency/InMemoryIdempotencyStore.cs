using System.Collections.Concurrent;
using Backend.Application.Services;

namespace Backend.Infrastructure.Idempotency;

public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private static readonly ConcurrentDictionary<string, CacheEntry> _cache = new();

    public Task<(bool Exists, object? Response)> TryGetAsync(string key, CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow && !entry.IsInProgress)
        {
            return Task.FromResult((true, entry.Response));
        }
        return Task.FromResult((false, (object?)null));
    }

    public Task SetAsync(string key, object? response, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        _cache[key] = new CacheEntry(response, DateTime.UtcNow.Add(expiry), false);
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.TryRemove(key, out _);
        return Task.CompletedTask;
    }

    public Task<bool> TryMarkInProgressAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default)
    {
        var entry = new CacheEntry(null, DateTime.UtcNow.Add(expiry), true);
        return Task.FromResult(_cache.TryAdd(key, entry));
    }

    private sealed record CacheEntry(object? Response, DateTime ExpiresAt, bool IsInProgress);
}
