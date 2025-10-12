namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Request model for user registration
/// Maps to User entity for creating a new account
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// User's email address - must be valid and unique
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User's password - will be hashed before storage
    /// </summary>
    public required string Password { get; init; }
}
