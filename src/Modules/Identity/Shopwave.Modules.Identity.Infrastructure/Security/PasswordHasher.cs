using BCrypt.Net;
using Shopwave.Modules.Identity.Application.Abstractions;

namespace Shopwave.Modules.Identity.Infrastructure.Security;

public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        
        string passwordHash = BCrypt.Net.BCrypt.HashPassword(password, workFactor: 13);
        return passwordHash;
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(password))
            throw new ArgumentException("Password cannot be empty.", nameof(password));
        
        bool isPasswordCorrect = BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        return isPasswordCorrect;
    }
}