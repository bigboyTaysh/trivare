namespace Trivare.Application.DTOs.Auth;

/// <summary>
/// Response model for successful user registration
/// Derived from User entity, excludes sensitive fields
/// </summary>
public record RegisterResponse
{
    /// <summary>
    /// Unique user identifier from User.Id
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// User's email address from User.Email
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Account creation timestamp from User.CreatedAt
    /// </summary>
    public required DateTime CreatedAt { get; init; }
}
