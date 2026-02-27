namespace Backend.Application.Configuration;

public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;
    public int WindowSeconds { get; set; } = 60;
}
