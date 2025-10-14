using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Users;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for user-related operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Gets the current authenticated user's profile information
    /// </summary>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The user's profile data</returns>
    Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default);
}