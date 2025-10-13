namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Result type for ResetPassword operation to avoid exception-driven flow
/// </summary>
public record ResetPasswordResult
{
    public bool Success { get; init; }
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }
}
