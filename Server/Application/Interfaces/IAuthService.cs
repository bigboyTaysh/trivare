using Trivare.Application.DTOs.Auth;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for authentication-related operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Registers a new user account
    /// </summary>
    /// <param name="request">Registration details including email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Registration response with user information</returns>
    /// <exception cref="Exceptions.EmailAlreadyExistsException">Thrown when email is already registered</exception>
    Task<RegisterResponse> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Authenticates a user with email and password credentials
    /// Returns JWT access and refresh tokens upon successful authentication
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with JWT tokens and user information</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when credentials are invalid</exception>
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
