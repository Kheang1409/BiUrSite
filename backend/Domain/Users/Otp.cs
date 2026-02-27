namespace Backend.Domain.Users;

public record Otp
{
    private const int MIN = 100000;
    private const int MAX = 999999;
    private const int MIN_TO_EXPIRE = 3;
    public string Value { get; init; } = string.Empty;
    public DateTime ExpireAt { get; init; }
    private Otp(){}
    private Otp(string value, DateTime expiredAt)
    {
        Value = value;
        ExpireAt = expiredAt;
    }
    public static Otp Generate()
    {
        var value = new Random().Next(MIN, MAX).ToString();
        var expiredAt = DateTime.UtcNow.AddMinutes(MIN_TO_EXPIRE);

        return new Otp(value, expiredAt);
    }
}