namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for initiating password reset
/// Triggers email with reset token
/// </summary>
public record ForgotPasswordRequest
{
    /// <summary>
    /// Email address of the account to reset
    /// </summary>
    public required string Email { get; init; }
}
