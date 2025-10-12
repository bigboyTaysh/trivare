namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for refreshing access token
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// Valid refresh token to exchange for new access token
    /// </summary>
    public required string RefreshToken { get; init; }
}
