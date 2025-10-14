using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Common;

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
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Authenticates a user with email and password credentials
    /// Returns JWT access and refresh tokens upon successful authentication
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Login response with JWT tokens and user information</returns>
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refreshes access token using a valid refresh token
    /// Validates the refresh token and generates new access and refresh tokens
    /// </summary>
    /// <param name="request">Refresh token request containing the refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Refresh token response with new JWT tokens</returns>
    Task<Result<RefreshTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// Validates the refresh token and marks it as invalidated
    /// </summary>
    /// <param name="request">Logout request containing the refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Logout response with success message</returns>
    Task<Result<LogoutResponse>> LogoutAsync(LogoutRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Resets a user's password using a reset token
    /// Validates the token, current password, updates the password, and invalidates the token
    /// </summary>
    /// <param name="request">Reset password request with token, current and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result indicating success or failure and an error code/message for expected failures</returns>
    Task<Result<string>> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken = default);
}
