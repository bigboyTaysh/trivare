using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for user registration
/// Maps to User entity for creating a new account
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// User's username - must be unique
    /// </summary>
    [Required(ErrorMessage = "Username is required")]
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
    public required string UserName { get; init; }

    /// <summary>
    /// User's email address - must be valid and unique
    /// </summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [MaxLength(255, ErrorMessage = "Email must not exceed 255 characters")]
    public required string Email { get; init; }

    /// <summary>
    /// User's password - will be hashed before storage
    /// Must be at least 8 characters and contain uppercase, lowercase, number, and special character
    /// </summary>
    [Required(ErrorMessage = "Password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&.])[A-Za-z\d@$!%*?&.]+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
    public required string Password { get; init; }
}
