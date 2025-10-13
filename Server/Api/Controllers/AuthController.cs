using Microsoft.AspNetCore.Mvc;
using Trivare.Application.DTOs.Auth;
using Trivare.Application.DTOs.Common;
using Trivare.Application.Exceptions;
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
        try
        {
            var response = await _authService.RegisterAsync(request, cancellationToken);
            
            // Return 201 Created with Location header
            return CreatedAtAction(
                actionName: nameof(Register),
                routeValues: new { id = response.Id },
                value: response
            );
        }
        catch (EmailAlreadyExistsException ex)
        {
            _logger.LogWarning(ex, "Registration failed - email already exists");
            return Conflict(new ErrorResponse
            {
                Error = "EmailAlreadyExists",
                Message = ex.Message
            });
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Role"))
        {
            _logger.LogCritical(ex, "Default 'User' role not found in database");
            return StatusCode(500, new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An error occurred while processing your request. Please try again later."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during user registration");
            return StatusCode(500, new ErrorResponse
            {
                Error = "InternalServerError",
                Message = "An error occurred while processing your request. Please try again later."
            });
        }
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
        try
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // User not found or invalid password - same message for security
            return Unauthorized(new ErrorResponse
            {
                Error = "InvalidCredentials",
                Message = "Invalid email or password"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login for email: {Email}", request.Email);
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Error = "InternalError",
                Message = "An error occurred during login. Please try again later."
            });
        }
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
        try
        {
            var response = await _authService.RefreshTokenAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // Invalid or expired refresh token
            return Unauthorized(new ErrorResponse
            {
                Error = "InvalidRefreshToken",
                Message = "Invalid or expired refresh token"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh");
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Error = "InternalError",
                Message = "An error occurred during token refresh. Please try again later."
            });
        }
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
    [ProducesResponseType(typeof(LogoutResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LogoutResponseDto>> Logout(
        [FromBody] LogoutRequestDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await _authService.LogoutAsync(request, cancellationToken);
            return Ok(response);
        }
        catch (UnauthorizedAccessException)
        {
            // Invalid refresh token
            return Unauthorized(new ErrorResponse
            {
                Error = "InvalidRefreshToken",
                Message = "Invalid refresh token provided"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout");
            
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Error = "InternalError",
                Message = "An error occurred during logout. Please try again later."
            });
        }
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

        if (result.Success)
        {
            return Ok(new { message = result.Message ?? "Password reset successful" });
        }

        // Map expected failure codes to HTTP responses
        var message = result.Message ?? "An error occurred";
        return result.ErrorCode switch
        {
            "TokenNotFound" => NotFound(new ErrorResponse { Error = "TokenNotFound", Message = message }),
            "TokenExpired" => BadRequest(new ErrorResponse { Error = "TokenExpired", Message = message }),
            "CurrentPasswordMismatch" => BadRequest(new ErrorResponse { Error = "CurrentPasswordMismatch", Message = message }),
            "SamePassword" => BadRequest(new ErrorResponse { Error = "SamePassword", Message = message }),
            _ => StatusCode(500, new ErrorResponse { Error = "InternalServerError", Message = message })
        };
    }
}
