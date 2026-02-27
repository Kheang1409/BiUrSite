using System.Security.Cryptography;
using System.Text;
using Backend.Application.Services;

namespace Backend.Infrastructure.Authentication;

public sealed class Sha512PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;

    public string HashPassword(string plainPassword)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainPassword);

        using var sha512 = SHA512.Create();
        
        byte[] salt = new byte[SaltSize];
        RandomNumberGenerator.Fill(salt);

        byte[] passwordBytes = Encoding.UTF8.GetBytes(plainPassword);
        byte[] saltedPassword = [.. passwordBytes, .. salt];
        byte[] hashBytes = sha512.ComputeHash(saltedPassword);
        byte[] hashWithSalt = [.. salt, .. hashBytes];

        return Convert.ToBase64String(hashWithSalt);
    }

    public bool VerifyPassword(string plainPassword, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(plainPassword) || string.IsNullOrWhiteSpace(hashedPassword))
            return false;

        try
        {
            byte[] hashWithSalt = Convert.FromBase64String(hashedPassword);
            
            if (hashWithSalt.Length < SaltSize)
                return false;

            byte[] salt = new byte[SaltSize];
            Array.Copy(hashWithSalt, 0, salt, 0, SaltSize);

            byte[] storedHash = new byte[hashWithSalt.Length - SaltSize];
            Array.Copy(hashWithSalt, SaltSize, storedHash, 0, storedHash.Length);

            using var sha512 = SHA512.Create();
            byte[] passwordBytes = Encoding.UTF8.GetBytes(plainPassword);
            byte[] saltedPassword = [.. passwordBytes, .. salt];
            byte[] computedHash = sha512.ComputeHash(saltedPassword);

            return CryptographicOperations.FixedTimeEquals(storedHash, computedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
