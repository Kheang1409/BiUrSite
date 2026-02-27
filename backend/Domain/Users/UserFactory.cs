using Backend.Domain.Enums;

namespace Backend.Domain.Users;

public class UserFactory : IStandardUserFactory
{
    public User Create(UserId? id, string username, string email, string? password, string provider)
    {
        _ = provider;
        var user = new User.Builder()
                    .SetUserName(username)
                    .SetEmail(email)
                    .SetPassword(password)
                    .SetToken(Token.Generate())
                    .SetStatus(Status.Unverified)
                    .Build();
        return user;
    }
}
