using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Trivare.Api.Extensions;

/// <summary>
/// Extension methods for ControllerBase
/// </summary>
public static class ControllerExtensions
{
    /// <summary>
    /// Extracts and validates the authenticated user's ID from JWT claims
    /// </summary>
    /// <param name="controller">The controller instance</param>
    /// <returns>The authenticated user's GUID</returns>
    /// <exception cref="UnauthorizedAccessException">Thrown when user ID is invalid or missing</exception>
    public static Guid GetAuthenticatedUserId(this ControllerBase controller)
    {
        var userIdClaim = controller.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid or missing user ID in authentication token");
        }
        
        return userId;
    }
}
