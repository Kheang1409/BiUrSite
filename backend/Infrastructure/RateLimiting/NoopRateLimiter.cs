using Backend.Application.Services;

namespace Backend.Infrastructure.RateLimiting;

public class NoopRateLimiter : IRateLimiter
{
    public Task<bool> ShouldAllowAsync(string key, int limit, TimeSpan window, CancellationToken ct = default)
        => Task.FromResult(true);
}
