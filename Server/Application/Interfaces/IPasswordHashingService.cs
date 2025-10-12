namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for secure password hashing and verification
/// </summary>
public interface IPasswordHashingService
{
    /// <summary>
    /// Hashes a password using PBKDF2 with a randomly generated salt
    /// </summary>
    /// <param name="password">The plaintext password to hash</param>
    /// <returns>A tuple containing the hash and salt as byte arrays</returns>
    (byte[] Hash, byte[] Salt) HashPassword(string password);

    /// <summary>
    /// Verifies a password against a stored hash and salt
    /// </summary>
    /// <param name="password">The plaintext password to verify</param>
    /// <param name="hash">The stored hash to compare against</param>
    /// <param name="salt">The salt used to generate the stored hash</param>
    /// <returns>True if the password matches, false otherwise</returns>
    bool VerifyPassword(string password, byte[] hash, byte[] salt);
}
