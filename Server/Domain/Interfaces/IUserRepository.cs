using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for User entity operations
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Gets a user by email address, including their roles
    /// </summary>
    /// <param name="email">The email address to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a user by ID, including their roles
    /// </summary>
    /// <param name="id">The user ID to search for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user if found, null otherwise</returns>
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if an email address is already registered
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if email exists, false otherwise</returns>
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new user
    /// </summary>
    /// <param name="user">The user entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created user with generated ID</returns>
    Task<User> AddAsync(User user, CancellationToken cancellationToken = default);
}
