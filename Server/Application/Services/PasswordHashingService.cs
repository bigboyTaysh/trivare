using System.Security.Cryptography;
using Trivare.Application.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service for secure password hashing using PBKDF2-HMAC-SHA256
/// Implements OWASP recommendations for password storage
/// </summary>
public class PasswordHashingService : IPasswordHashingService
{
    private const int SaltSize = 128; // bytes
    private const int HashSize = 256; // bytes
    private const int Iterations = 100000; // OWASP 2024 recommendation

    /// <summary>
    /// Hashes a password using PBKDF2 with a randomly generated salt
    /// </summary>
    /// <param name="password">The plaintext password to hash</param>
    /// <returns>A tuple containing the hash and salt as byte arrays</returns>
    public (byte[] Hash, byte[] Salt) HashPassword(string password)
    {
        // Generate cryptographically strong random salt
        byte[] salt = new byte[SaltSize];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(salt);
        }
        
        // Generate hash using PBKDF2
        using (var pbkdf2 = new Rfc2898DeriveBytes(
            password, 
            salt, 
            Iterations, 
            HashAlgorithmName.SHA256))
        {
            byte[] hash = pbkdf2.GetBytes(HashSize);
            return (hash, salt);
        }
    }

    /// <summary>
    /// Verifies a password against a stored hash and salt
    /// Uses constant-time comparison to prevent timing attacks
    /// </summary>
    /// <param name="password">The plaintext password to verify</param>
    /// <param name="hash">The stored hash to compare against</param>
    /// <param name="salt">The salt used to generate the stored hash</param>
    /// <returns>True if the password matches, false otherwise</returns>
    public bool VerifyPassword(string password, byte[] hash, byte[] salt)
    {
        using (var pbkdf2 = new Rfc2898DeriveBytes(
            password, 
            salt, 
            Iterations, 
            HashAlgorithmName.SHA256))
        {
            byte[] computedHash = pbkdf2.GetBytes(HashSize);
            return CryptographicOperations.FixedTimeEquals(computedHash, hash);
        }
    }
}
