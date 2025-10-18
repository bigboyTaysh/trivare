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
public class AccommodationController : ControllerBase
{
    private readonly IAccommodationService _accommodationService;

    public AccommodationController(IAccommodationService accommodationService)
    {
        _accommodationService = accommodationService;
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