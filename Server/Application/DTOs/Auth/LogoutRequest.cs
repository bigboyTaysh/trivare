using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request DTO for logout operation
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// JWT refresh token to invalidate
    /// </summary>
    [Required(ErrorMessage = "Refresh token is required")]
    public string RefreshToken { get; set; } = string.Empty;
}