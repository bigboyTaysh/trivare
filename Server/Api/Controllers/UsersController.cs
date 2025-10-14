using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Users;
using Trivare.Application.Interfaces;
using System.Security.Claims;

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
        // Extract user ID from JWT claims - "sub" is mapped to NameIdentifier
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("Invalid or missing user ID in JWT claims");
            return Unauthorized(new ErrorResponse { Error = "Unauthorized", Message = "Invalid authentication token" });
        }

        var result = await _userService.GetCurrentUserAsync(userId, cancellationToken);
        
        if (result.IsFailure)
        {
            _logger.LogWarning("User not found: {UserId}", userId);
            return NotFound(result.Error);
        }

        return Ok(result.Value);
    }
}