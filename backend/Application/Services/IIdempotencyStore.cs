namespace Backend.Application.Services;

public interface IIdempotencyStore
{
    Task<(bool Exists, object? Response)> TryGetAsync(string key, CancellationToken cancellationToken = default);
    Task SetAsync(string key, object? response, TimeSpan expiry, CancellationToken cancellationToken = default);
    Task RemoveAsync(string key, CancellationToken cancellationToken = default);
    Task<bool> TryMarkInProgressAsync(string key, TimeSpan expiry, CancellationToken cancellationToken = default);
}
