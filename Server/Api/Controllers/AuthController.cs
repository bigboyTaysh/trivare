using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Common;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for authentication operations
/// </summary>
[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ILogger<AuthController> _logger;

    public AuthController(IAuthService authService, ILogger<AuthController> logger)
    {
        _authService = authService;
        _logger = logger;
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    /// <param name="request">Registration details including email and password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created user information</returns>
    /// <response code="201">User successfully registered</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="409">Email already exists</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RegisterAsync(request, cancellationToken);
        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Authenticate user and receive JWT tokens
    /// </summary>
    /// <param name="request">Login credentials (email and password)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>JWT tokens and user information</returns>
    /// <response code="200">Login successful</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LoginAsync(request, cancellationToken);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Refresh access token using a valid refresh token
    /// </summary>
    /// <param name="request">Refresh token request containing the refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>New JWT tokens</returns>
    /// <response code="200">Token refresh successful</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Invalid or expired refresh token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.RefreshTokenAsync(request, cancellationToken);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Logs out a user by invalidating their refresh token
    /// </summary>
    /// <param name="request">Logout request containing the refresh token</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message indicating logout was completed</returns>
    /// <response code="200">Logout successful</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Invalid refresh token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LogoutResponse>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.LogoutAsync(request, cancellationToken);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Reset user password using reset token
    /// </summary>
    /// <param name="request">Reset password details including token and new password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Success message</returns>
    /// <response code="200">Password reset successful</response>
    /// <response code="400">Invalid token, expired token, or weak password</response>
    /// <response code="404">Token not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ResetPasswordAsync(request, cancellationToken);
        return this.HandleResult(result);
    }

    /// <summary>
    /// Initiate password recovery for a user
    /// </summary>
    /// <param name="request">Forgot password request containing the user's email</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A message indicating the result of the operation</returns>
    /// <response code="200">Password reset link sent to your email</response>
    /// <response code="400">Invalid email format</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(string), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<string>> ForgotPassword(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _authService.ForgotPasswordAsync(request, cancellationToken);
        return this.HandleResult(result);
    }
}
