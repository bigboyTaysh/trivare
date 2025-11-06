using Trivare.Domain.Entities;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for JWT token generation and management
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generates a JWT access token for authenticated user
    /// Token includes user ID, email, and roles as claims
    /// </summary>
    /// <param name="user">User entity with roles loaded</param>
    /// <returns>JWT access token string</returns>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generates a JWT refresh token for token renewal
    /// Token includes user ID and refresh token type
    /// </summary>
    /// <param name="userId">User identifier</param>
    /// <returns>JWT refresh token string</returns>
    string GenerateRefreshToken(Guid userId);

    /// <summary>
    /// Validates a refresh token and returns the user ID if valid
    /// </summary>
    /// <param name="token">Refresh token to validate</param>
    /// <returns>User ID if token is valid, null otherwise</returns>
    Guid? ValidateRefreshToken(string token);

    /// <summary>
    /// Gets the access token expiration time in seconds
    /// </summary>
    int GetAccessTokenExpiresIn();

    /// <summary>
    /// Gets the refresh token expiration time in days
    /// </summary>
    int GetRefreshTokenExpiresInDays();
}