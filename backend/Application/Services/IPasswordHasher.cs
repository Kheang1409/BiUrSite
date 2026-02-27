namespace Backend.Application.Services;

public interface IPasswordHasher
{
    string HashPassword(string plainPassword);

    bool VerifyPassword(string plainPassword, string hashedPassword);
}
