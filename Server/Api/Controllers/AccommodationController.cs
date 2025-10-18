using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Accommodation;
using Trivare.Application.DTOs.Common;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for accommodation-related operations
/// </summary>
[ApiController]
[Route("api/trips/{tripId}/accommodation")]
[Authorize]
[Produces("application/json")]
public class AccommodationController : ControllerBase
{
    private readonly IAccommodationService _accommodationService;

    public AccommodationController(IAccommodationService accommodationService)
    {
        _accommodationService = accommodationService;
    }

    /// <summary>
    /// Add accommodation to a specific trip
    /// </summary>
    /// <param name="tripId">The unique identifier of the trip</param>
    /// <param name="request">Accommodation data to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created accommodation information</returns>
    /// <response code="201">Accommodation added successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - trip belongs to another user</response>
    /// <response code="404">Trip not found</response>
    /// <response code="409">Accommodation already exists for this trip</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(AccommodationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateAccommodation(Guid tripId, CreateAccommodationRequest request, CancellationToken cancellationToken = default)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _accommodationService.AddAccommodationAsync(request, tripId, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Updates accommodation details for a specific trip
    /// Supports partial updates - only provided fields are modified
    /// </summary>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="request">The accommodation update request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated accommodation details</returns>
    [HttpPatch]
    [ProducesResponseType(typeof(AccommodationDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateAccommodation(Guid tripId, UpdateAccommodationRequest request, CancellationToken cancellationToken = default)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _accommodationService.UpdateAccommodationAsync(request, tripId, userId, cancellationToken);

        return this.HandleResult(result);
    }
}