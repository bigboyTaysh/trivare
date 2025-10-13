using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Users;
using Trivare.Application.Exceptions;
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

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHashingService passwordHashingService,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHashingService = passwordHashingService;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
    }

    /// <summary>
    /// Registers a new user account
    /// </summary>
    public async Task<RegisterResponse> RegisterAsync(
        RegisterRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Normalize email to lowercase and trim whitespace
        var email = request.Email.Trim().ToLowerInvariant();

        // Check if email already exists
        if (await _userRepository.EmailExistsAsync(email, cancellationToken))
        {
            _logger.LogWarning("Registration failed - email already exists: {Email}", email);

            throw new EmailAlreadyExistsException(email);
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
            Email = email,
            PasswordHash = hash,
            PasswordSalt = salt,
            CreatedAt = DateTime.UtcNow
        };

        // Assign default role
        // Only set foreign keys - don't set navigation properties to avoid EF tracking issues
        var userRole = new UserRole
        {
            UserId = user.Id,
            RoleId = defaultRole.Id
        };
        user.UserRoles.Add(userRole);

        // Save to database
        try
        {
            await _userRepository.AddAsync(user, cancellationToken);
            _logger.LogInformation("User registered successfully: {Email}, UserId: {UserId}", 
                user.Email, user.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database error during user registration for email: {Email}", email);
            
            throw;
        }

        // Log successful registration
        await LogAuditAsync(user.Id, "UserRegistered", new
        {
            Email = user.Email
        }, cancellationToken);

        // Map to response DTO
        return new RegisterResponse
        {
            Id = user.Id,
            Email = user.Email,
            CreatedAt = user.CreatedAt
        };
    }

    /// <summary>
    /// Authenticates a user with email and password
    /// </summary>
    public async Task<LoginResponse> LoginAsync(
        LoginRequest request, 
        CancellationToken cancellationToken = default)
    {
        // Normalize email to lowercase and trim whitespace
        var email = request.Email.Trim().ToLowerInvariant();

        try
        {
            // Fetch user with roles
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            
            if (user == null)
            {
                _logger.LogWarning("Login failed - user not found: {Email}", email);
                
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Verify password
            if (!_passwordHashingService.VerifyPassword(request.Password, user.PasswordHash, user.PasswordSalt))
            {
                _logger.LogWarning("Login failed - invalid password: {Email}, UserId: {UserId}", 
                    email, user.Id);
                
                throw new UnauthorizedAccessException("Invalid credentials");
            }

            // Generate JWT tokens
            var accessToken = _jwtTokenService.GenerateAccessToken(user);
            var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            _logger.LogInformation("User logged in successfully: {Email}, UserId: {UserId}", 
                user.Email, user.Id);

            // Map to UserDto
            var userDto = new UserDto
            {
                Id = user.Id,
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
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw authentication errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", email);
            
            throw;
        }
    }

    /// <summary>
    /// Refreshes access token using a valid refresh token
    /// </summary>
    public async Task<RefreshTokenResponse> RefreshTokenAsync(
        RefreshTokenRequest request, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Validate the refresh token
            var userId = _jwtTokenService.ValidateRefreshToken(request.RefreshToken);
            if (userId == null)
            {
                _logger.LogWarning("Refresh token validation failed - invalid token");
                
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Fetch user by ID
            var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);
            if (user == null)
            {
                _logger.LogWarning("Refresh token validation failed - user not found: UserId {UserId}", userId);
                
                throw new UnauthorizedAccessException("Invalid refresh token");
            }

            // Generate new tokens
            var newAccessToken = _jwtTokenService.GenerateAccessToken(user);
            var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

            _logger.LogInformation("Refresh token successful: UserId {UserId}", user.Id);

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                ExpiresIn = _jwtTokenService.GetAccessTokenExpiresIn()
            };
        }
        catch (UnauthorizedAccessException)
        {
            throw; // Re-throw authentication errors
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during refresh token");
            
            throw;
        }
    }

    /// <summary>
    /// Helper method to create audit log entries
    /// Failures in audit logging don't break the main operation
    /// </summary>
    private async Task LogAuditAsync(
        Guid? userId, 
        string eventType, 
        object details, 
        CancellationToken cancellationToken)
    {
        try
        {
            var auditLog = new AuditLog
            {
                UserId = userId,
                EventType = eventType,
                EventTimestamp = DateTime.UtcNow,
                Details = JsonSerializer.Serialize(details)
            };

            await _auditLogRepository.AddAsync(auditLog, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to write audit log for event: {EventType}", eventType);
            // Don't throw - audit logging failure shouldn't break registration
        }
    }
}
