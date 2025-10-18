using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Users;

/// <summary>
/// Request model for updating user profile
/// Partial update of User entity
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// New username - must be unique
    /// Updates User.UserName
    /// </summary>
    [MinLength(3, ErrorMessage = "Username must be at least 3 characters")]
    [MaxLength(50, ErrorMessage = "Username must not exceed 50 characters")]
    [RegularExpression(@"^[a-zA-Z0-9_-]+$", ErrorMessage = "Username can only contain letters, numbers, underscores and hyphens")]
    public string? UserName { get; init; }

    /// <summary>
    /// Current password - required when changing password
    /// </summary>
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    public string? CurrentPassword { get; init; }

    /// <summary>
    /// New password - must meet security requirements
    /// Updates User.PasswordHash and User.PasswordSalt
    /// </summary>
    [MinLength(8, ErrorMessage = "New password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "New password must contain uppercase, lowercase, number, and special character")]
    public string? NewPassword { get; init; }
}
