namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Response model for token refresh operation
/// Contains new access and refresh tokens
/// </summary>
public record RefreshTokenResponse
{
    /// <summary>
    /// New JWT access token for API authorization
    /// </summary>
    public required string AccessToken { get; init; }

    /// <summary>
    /// New JWT refresh token
    /// </summary>
    public required string RefreshToken { get; init; }

    /// <summary>
    /// New access token expiration time in seconds
    /// </summary>
    public required int ExpiresIn { get; init; }
}
