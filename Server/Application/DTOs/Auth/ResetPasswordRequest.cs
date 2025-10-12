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
    public required string Token { get; init; }

    /// <summary>
    /// New password to set - will be hashed before storage
    /// </summary>
    public required string NewPassword { get; init; }
}
