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
    public required string Email { get; init; }

    /// <summary>
    /// User's password for authentication
    /// </summary>
    public required string Password { get; init; }
}
