namespace Trivare.Application.DTOs.Users;

/// <summary>
/// Request model for updating user profile
/// Partial update of User entity
/// </summary>
public record UpdateUserRequest
{
    /// <summary>
    /// New email address - must be valid and unique
    /// Updates User.Email
    /// </summary>
    public required string Email { get; init; }
}
