namespace Trivare.Application.DTOs.Users;

/// <summary>
/// User profile data transfer object
/// Derived from User entity with UserRoles joined to get role names
/// Excludes sensitive fields (password hash, salt, reset tokens)
/// </summary>
public record UserDto
{
    /// <summary>
    /// User identifier from User.Id
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

    /// <summary>
    /// Array of role names assigned to user
    /// Derived from User.UserRoles -> Role.Name
    /// </summary>
    public required IEnumerable<string> Roles { get; init; }
}
