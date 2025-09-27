using Backend.Domain.Enums;

namespace Backend.Domain.Users;
public class OAuthUserFactory : IUserFactory
{
    public User Create(UserId? Id, string username, string email, string password, string provider)
    {
        var user = new User.Builder()
            .SetId(Id)
            .SetUserName(username)
            .SetEmail(email)
            .SetAuthProvider(provider)
            .SetStatus(Status.Active)
            .Build();
            
        return user;
    }
}
