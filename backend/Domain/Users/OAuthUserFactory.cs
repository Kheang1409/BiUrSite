using Backend.Domain.Enums;

namespace Backend.Domain.Users;
public class OAuthUserFactory : IOAuthUserFactory
{
    public User Create(UserId? id, string username, string email, string? password, string provider)
    {
        var user = new User.Builder()
            .SetId(id)
            .SetUserName(username)
            .SetEmail(email)
            .SetAuthProvider(provider)
            .SetStatus(Status.Active)
            .Build();
            
        return user;
    }
}
