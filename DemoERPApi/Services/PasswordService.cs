using DemoERPApi.Models;
using Microsoft.AspNetCore.Identity;

namespace DemoERPApi.Services;


/// Service for password hashing and verification using ASP.NET Core Identity.
/// Uses PBKDF2 with HMAC-SHA256 and automatic salting for secure password storage.

public class PasswordService
{
    private readonly PasswordHasher<User> _passwordHasher;

    public PasswordService()
    {
        _passwordHasher = new PasswordHasher<User>();
    }

    
    /// Hashes a plain text password with a random salt.
    
    /// <param name="user">User entity for hashing context</param>
    /// <param name="password">Plain text password</param>
    /// <returns>Hashed password string</returns>
    public string HashPassword(User user, string password)
    {
        return _passwordHasher.HashPassword(user, password);
    }

    
    /// Verifies a password against a stored hash.
    
    /// <param name="user">User entity for verification context</param>
    /// <param name="hashedPassword">Stored password hash</param>
    /// <param name="password">Plain text password to verify</param>
    /// <returns>True if password matches, false otherwise</returns>
    public bool VerifyPassword(User user, string hashedPassword, string password)
    {
        var result = _passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
        return result == PasswordVerificationResult.Success;
    }
}