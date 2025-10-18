using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for user authentication
/// Contains credentials to validate against User entity
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// User's email address
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email cannot exceed 255 characters")]
    public required string Email { get; init; }

    /// <summary>
    /// User's password for authentication
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    public required string Password { get; init; }
}
