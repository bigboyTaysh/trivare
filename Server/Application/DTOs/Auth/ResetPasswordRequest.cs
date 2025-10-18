using System.ComponentModel.DataAnnotations;

namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for completing password reset
/// Uses token from email to set new password
/// Maps to User.PasswordResetToken and User.PasswordHash
/// </summary>
public record ResetPasswordRequest
{
    /// <summary>
    /// Password reset token from email (stored in User.PasswordResetToken)
    /// </summary>
    [Required(ErrorMessage = "Token is required")]
    [MinLength(1, ErrorMessage = "Token cannot be empty")]
    public required string Token { get; init; }

    /// <summary>
    /// New password to set - will be hashed before storage
    /// Must be at least 8 characters and contain uppercase, lowercase, number, and special character
    /// </summary>
    [Required(ErrorMessage = "New password is required")]
    [MinLength(8, ErrorMessage = "Password must be at least 8 characters")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$",
        ErrorMessage = "Password must contain uppercase, lowercase, number, and special character")]
    public required string NewPassword { get; init; }
}
