using System.Security.Cryptography;

namespace Backend.API.Helpers;


public class Utility
{

    static MD5 provider = MD5.Create();
    public static Guid StringToGuid(string input)
    {
        var hash = provider.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}