using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for refresh token operation
/// Contains the refresh token to validate and exchange for new tokens
/// </summary>
public record RefreshTokenRequest
{
    /// <summary>
    /// JWT refresh token for obtaining new access tokens
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public required string RefreshToken { get; init; }
}
