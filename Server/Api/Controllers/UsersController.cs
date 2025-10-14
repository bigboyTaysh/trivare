using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Users;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for user profile operations
/// </summary>
[ApiController]
[Route("api/users")]
[Authorize]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    /// <summary>
    /// Get current authenticated user's profile
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>User profile information</returns>
    /// <response code="200">User profile retrieved successfully</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> GetMe(CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _userService.GetCurrentUserAsync(userId, cancellationToken);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Update current authenticated user's profile
    /// </summary>
    /// <param name="request">Update request containing new username and/or password</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated user profile information</returns>
    /// <response code="200">User profile updated successfully</response>
    /// <response code="400">Bad request - validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserDto>> UpdateMe([FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _userService.UpdateUserAsync(userId, request, cancellationToken);
        
        if (result.IsFailure)
        {
            var errorResponse = result.Error as ErrorResponse;
            if (errorResponse?.Error == AuthErrorCodes.CurrentPasswordMismatch)
            {
                return BadRequest(errorResponse);
            }
            
            _logger.LogWarning("User update failed: {UserId}, Error: {Error}", userId, errorResponse?.Error);
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }
}