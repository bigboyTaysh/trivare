using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for Role entity operations
/// </summary>
public interface IRoleRepository
{
    /// <summary>
    /// Gets a role by name
    /// </summary>
    /// <param name="roleName">The role name to search for (e.g., "User", "Admin")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The role if found, null otherwise</returns>
    Task<Role?> GetByNameAsync(string roleName, CancellationToken cancellationToken = default);
}
