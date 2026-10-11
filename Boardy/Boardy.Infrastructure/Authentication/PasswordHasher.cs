using Boardy.Domain.Authentication;
using System.Security.Cryptography;
using System.Text;

namespace Boardy.Infrastructure.Authentication;

public class PasswordHasher : IPasswordHasher
{
    public string Hash(string password) 
    {
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    public bool Verify(string password, string passwordHash)
    {
        var hashOfInput = Hash(password);
        return hashOfInput == passwordHash;
    }
}
