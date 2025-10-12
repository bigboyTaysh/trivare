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
}
