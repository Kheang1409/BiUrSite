namespace Backend.Domain.Users.Factories;

using Backend.Domain.Users.Entities;

public interface IUserFactory
{
    User Create(string username, string email, string password);
}
