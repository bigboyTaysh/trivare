using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Users;
using Trivare.Application.Interfaces;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for user-related operations
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;

    public UserService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    /// <summary>
    /// Gets the current authenticated user's profile information
    /// </summary>
    public async Task<Result<UserDto>> GetCurrentUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return new ErrorResponse { Error = UserErrorCodes.UserNotFound, Message = "User not found" };
        }

        return new UserDto
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
}