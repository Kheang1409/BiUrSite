namespace Backend.Domain.Users;

public interface IUserFactory
{
    User Create(UserId? Id, string username, string email, string? password, string provider);
}
