using Backend.Domain.Enums;

namespace Backend.Domain.Users;

public class UserFactory : IUserFactory
{
    public User Create(UserId? Id, string username, string email, string password, string provider)
    {
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
