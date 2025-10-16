namespace Backend.Application.Services;

public interface IRateLimiter
{
    // Returns true if the request is allowed; false if it should be throttled.
    Task<bool> ShouldAllowAsync(string key, int limit, TimeSpan window, CancellationToken ct = default);
}
