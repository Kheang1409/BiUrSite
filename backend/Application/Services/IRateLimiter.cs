namespace Backend.Application.Services;

public interface IRateLimiter
{
    Task<bool> ShouldAllowAsync(string key, int limit, TimeSpan window, CancellationToken ct = default);
}
