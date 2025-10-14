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
    private readonly IPasswordHashingService _passwordHashingService;

    public UserService(IUserRepository userRepository, IPasswordHashingService passwordHashingService)
    {
        _userRepository = userRepository;
        _passwordHashingService = passwordHashingService;
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
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }

    /// <summary>
    /// Updates the current authenticated user's profile
    /// </summary>
    public async Task<Result<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(userId, cancellationToken);

        if (user == null)
        {
            return new ErrorResponse { Error = UserErrorCodes.UserNotFound, Message = "User not found" };
        }

        // Update username if provided
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            user.UserName = request.UserName.Trim();
        }

        // Update password if provided
        if (!string.IsNullOrWhiteSpace(request.NewPassword))
        {
            // Validate current password is provided
            if (string.IsNullOrWhiteSpace(request.CurrentPassword))
            {
                return new ErrorResponse { Error = AuthErrorCodes.CurrentPasswordMismatch, Message = "Current password is required to change password" };
            }

            // Verify current password
            if (!_passwordHashingService.VerifyPassword(request.CurrentPassword, user.PasswordHash, user.PasswordSalt))
            {
                return new ErrorResponse { Error = AuthErrorCodes.CurrentPasswordMismatch, Message = "Current password is incorrect" };
            }

            // Hash new password
            var (hash, salt) = _passwordHashingService.HashPassword(request.NewPassword);
            user.PasswordHash = hash;
            user.PasswordSalt = salt;
        }

        await _userRepository.UpdateAsync(user, cancellationToken);

        return new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role.Name).ToList()
        };
    }
}