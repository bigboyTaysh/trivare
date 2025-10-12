using System.Text.Json;
using Microsoft.Extensions.Logging;
using Trivare.Application.DTOs.Auth;
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
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHashingService passwordHashingService,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHashingService = passwordHashingService;
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
            
            // Log failed registration attempt
            await LogAuditAsync(null, "RegistrationFailed", new
            {
                Email = email,
                Reason = "EmailAlreadyExists"
            }, cancellationToken);

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
            
            // Log failed registration attempt
            await LogAuditAsync(null, "RegistrationFailed", new
            {
                Email = email,
                Reason = "DatabaseError",
                ErrorMessage = ex.Message
            }, cancellationToken);

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
