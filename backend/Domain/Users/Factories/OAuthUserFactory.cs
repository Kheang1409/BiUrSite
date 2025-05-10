namespace Backend.Domain.Users.Factories;

using Backend.Domain.Users.Entities;

public class OAuthUserFactory : IUserFactory
{
    public User Create(string username, string email, string password)
    {
        var user = new User(username, email, password);
        user.Activate();
        return user;
    }
}
