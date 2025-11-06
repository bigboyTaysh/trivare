using Trivare.Application.DTOs.Users;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Response model for successful authentication
/// Contains JWT tokens and user information
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// JWT access token for API authorization (15 min lifetime)
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// JWT refresh token for obtaining new access tokens (7 day lifetime)
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// Access token expiration time in seconds (900 = 15 minutes)
    /// </summary>
    public required int ExpiresIn { get; init; }

    /// <summary>
    /// Authenticated user information from User entity
    /// </summary>
    public required UserDto User { get; init; }
}
