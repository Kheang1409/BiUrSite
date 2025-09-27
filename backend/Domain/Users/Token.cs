namespace Backend.Domain.Users;

public record Token
{
    private const int MIN_TO_EXPIRE = 24*60;
    public string Value { get; init;  }
    public DateTime ExpireAt { get; init; }

    private Token() { }
    private Token(string value, DateTime expiredAt)
    {
        Value = value;
        ExpireAt = expiredAt;
    }
    public static Token? Generate()
    {
        var value = Guid.NewGuid().ToString();
        var expiredAt = DateTime.UtcNow.AddMinutes(MIN_TO_EXPIRE);

        return new Token(value, expiredAt);
    }
}