using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Trips;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for trip operations
/// </summary>
[ApiController]
[Route("api/trips")]
[Authorize]
[Produces("application/json")]
public class TripsController : ControllerBase
{
    private readonly ITripService _tripService;

    public TripsController(ITripService tripService)
    {
        _tripService = tripService;
    }

    /// <summary>
    /// Create a new trip
    /// </summary>
    /// <param name="request">Trip creation data</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created trip information</returns>
    /// <response code="201">Trip created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="409">Trip limit exceeded</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpPost]
    [ProducesResponseType(typeof(CreateTripResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTripResponse>> CreateTrip([FromBody] CreateTripRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _tripService.CreateTripAsync(request, userId, cancellationToken);

        return this.HandleResult(result);
    }
}