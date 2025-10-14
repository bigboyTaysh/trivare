using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Users;
using Trivare.Application.Interfaces;
using Trivare.Domain.Entities;
using Trivare.Domain.Interfaces;

namespace Trivare.Application.Services;

/// <summary>
/// Service implementation for authentication-related operations
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordHashingService _passwordHashingService;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IEmailService _emailService;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IEmailService emailService)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _emailService = emailService;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    public async Task<Result<RegisterResponse>> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Normalize email to lowercase and trim whitespace
        var email = request.Email.Trim().ToLowerInvariant();
        var userName = request.UserName.Trim();

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogWarning("Registration failed - email already exists: {Email}", email);
            return new ErrorResponse { Error = AuthErrorCodes.EmailAlreadyExists, Message = $"Email '{email}' is already registered." };
        }
        
        // Get default "User" role
        var defaultRole = await _roleRepository.GetByNameAsync("User", cancellationToken);
        if (defaultRole == null)
        {
            _logger.LogCritical("Default 'User' role not found in database");
            throw new InvalidOperationException("Default 'User' role not found");
        }

        // Hash password with salt
        var (hash, salt) = _passwordHashingService.HashPassword(request.Password);

        // Create user entity
        var user = new User
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        // Assign default role
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id
        };
        user.UserRoles.Add(userRole);

        await _userRepository.AddAsync(user, cancellationToken);

        var auditLogEntry = new AuditLog
        {
            UserId = user.Id,
            EventType = "UserRegistered",
            EventTimestamp = DateTime.UtcNow,
            Details = JsonSerializer.Serialize(new
            {
                Email = user.Email
            })
        };

        // Log successful registration
        await _auditLogRepository.AddAsync(auditLogEntry, cancellationToken);

        // Map to response DTO
        return new RegisterResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    public async Task<Result<LoginResponse>> LoginAsync(
        LoginRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Normalize email to lowercase and trim whitespace
        var email = request.Email.Trim().ToLowerInvariant();

        // Fetch user with roles
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        
        if (user == null)
        {
            _logger.LogWarning("Login failed - user not found: {Email}", email);
            return new ErrorResponse { Error = AuthErrorCodes.InvalidCredentials, Message = "Invalid email or password" };
        }

        // Verify password
        if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
        {
            _logger.LogWarning("Login failed - invalid password: {Email}, UserId: {UserId}", 
                email, user.Id);
            return new ErrorResponse { Error = AuthErrorCodes.InvalidCredentials, Message = "Invalid email or password" };
        }

        // Generate JWT tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        // Store refresh token in database
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtTokenService.GetRefreshTokenExpiresInDays());
        await _userRepository.UpdateAsync(user, cancellationToken);

        // Map to UserDto
        var userDto = new UserDto
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            CreatedAt = user.CreatedAt,
            Roles = user.UserRoles.Select(ur => ur.Role?.Name ?? "Unknown").ToList()
        };

        return new LoginResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            ExpiresIn = _jwtTokenService.GetAccessTokenExpiresIn(),
            User = userDto
        };
    }

    /// <summary>
    /// Refreshes access token using a valid refresh token
    /// </summary>
    public async Task<Result<RefreshTokenResponse>> RefreshTokenAsync(
        RefreshTokenRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Validate the refresh token
        var userId = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (userId == null)
        {
            _logger.LogWarning("Refresh token validation failed - invalid token");
            return new ErrorResponse { Error = AuthErrorCodes.InvalidRefreshToken, Message = "Invalid refresh token" };
        }

        // Fetch user by ID
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Refresh token validation failed - user not found: UserId {UserId}", userId);
            return new ErrorResponse { Error = AuthErrorCodes.InvalidRefreshToken, Message = "Invalid refresh token" };
        }

        // Check if the refresh token matches the stored one and is not expired
        if (user.RefreshToken != request.RefreshToken || 
            user.RefreshTokenExpiryTime == null || 
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
        {
            _logger.LogWarning("Refresh token validation failed - token mismatch or expired: UserId {UserId}", userId);
            return new ErrorResponse { Error = AuthErrorCodes.InvalidRefreshToken, Message = "Invalid or expired refresh token" };
        }

        // Generate new tokens
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        // Update refresh token in database
        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_jwtTokenService.GetRefreshTokenExpiresInDays());
        await _userRepository.UpdateAsync(user, cancellationToken);

        return new RefreshTokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _jwtTokenService.GetAccessTokenExpiresIn()
        };
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    public async Task<Result<LogoutResponse>> LogoutAsync(
        LogoutRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Validate the refresh token
        var userId = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
        if (userId == null)
        {
            _logger.LogWarning("Logout failed - invalid refresh token");
            return new ErrorResponse { Error = AuthErrorCodes.InvalidRefreshToken, Message = "Invalid refresh token provided" };
        }

        // Fetch user by ID
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Logout failed - user not found: UserId {UserId}", userId);
            return new ErrorResponse { Error = AuthErrorCodes.InvalidRefreshToken, Message = "Invalid refresh token provided" };
        }

        // Check if the refresh token matches the stored one
        if (user.RefreshToken != request.RefreshToken)
        {
            _logger.LogWarning("Logout failed - token mismatch: UserId {UserId}", userId);
            return new ErrorResponse { Error = AuthErrorCodes.InvalidRefreshToken, Message = "Invalid refresh token provided" };
        }

        // Invalidate the refresh token by clearing it from the user record
        user.RefreshToken = null;
        user.RefreshTokenExpiryTime = null;
        
        await _userRepository.UpdateAsync(user, cancellationToken);
        
        return new LogoutResponse
        {
            Message = "Logged out successfully"
        };
    }
    
    /// <summary>
    /// Resets a user's password using a reset token
    /// Returns a ResetPasswordResult indicating expected failure modes instead of throwing
    /// </summary>
    public async Task<Result<string>> ResetPasswordAsync(
        ResetPasswordRequest request,
        CancellationToken cancellationToken = default)
    {
        // Fetch user by reset token
        var user = await _userRepository.GetByPasswordResetTokenAsync(request.Token, cancellationToken);
        if (user == null)
        {
            _logger.LogWarning("Password reset failed - token not found: {Token}", request.Token);
            return new ErrorResponse { Error = AuthErrorCodes.TokenNotFound, Message = "Reset token not found" };
        }

        // Check if token is expired
        if (user.PasswordResetTokenExpiry.HasValue && 
            user.PasswordResetTokenExpiry.Value < DateTime.UtcNow)
        {
            _logger.LogWarning("Password reset failed - token expired: UserId {UserId}, Token {Token}", 
                user.Id, request.Token);
            return new ErrorResponse { Error = AuthErrorCodes.TokenExpired, Message = "Reset token has expired" };
        }

        // Check if new password is the same as current password
        if (_passwordHashingService.VerifyPassword(request.NewPassword, user.PasswordHash, user.PasswordSalt))
        {
            _logger.LogWarning("Password reset failed - new password same as current: UserId {UserId}", user.Id);
            return new ErrorResponse { Error = AuthErrorCodes.SamePassword, Message = "New password cannot be the same as the current password" };
        }

        // Hash new password
        var (newHash, newSalt) = _passwordHashingService.HashPassword(request.NewPassword);

        // Update user password and clear reset token
        user.PasswordHash = newHash;
        user.PasswordSalt = newSalt;
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;

        // Save changes
        await _userRepository.UpdateAsync(user, cancellationToken);

        return "Password reset successfully.";
    }

    public async Task<Result<string>> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email.Trim().ToLowerInvariant(), cancellationToken);

        if (user == null)
        {
            // To prevent email enumeration, always return a success-like message.
            _logger.LogInformation("Password reset requested for non-existent email: {Email}", request.Email);
            return "If an account with this email exists, a password reset link has been sent.";
        }

        // Generate a secure, URL-safe token
        var token = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(64));
        token = token.Replace('+', '-').Replace('/', '_'); // Make it URL-safe

        user.PasswordResetToken = token;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token is valid for 1 hour

        await _userRepository.UpdateAsync(user, cancellationToken);

        try
        {
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);
            _logger.LogInformation("Password reset email sent successfully to {Email}", user.Email);
        }
        catch (Exception ex)
        {
            // Log the error but don't fail the entire operation
            // Token is already stored in database for manual recovery if needed
            _logger.LogError(ex, "Failed to send password reset email to {Email}. Token stored in database.", user.Email);
            // Still return success message to prevent email enumeration
        }
        
        return "If an account with this email exists, a password reset link has been sent.";
    }
}
