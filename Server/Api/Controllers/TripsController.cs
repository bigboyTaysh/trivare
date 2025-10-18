using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Accommodation;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Transport;
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
    private readonly ITransportService _transportService;
    private readonly IAccommodationService _accommodationService;

    public TripsController(ITripService tripService, ITransportService transportService, IAccommodationService accommodationService)
    {
        _tripService = tripService;
        _transportService = transportService;
        _accommodationService = accommodationService;
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
    [HttpPost("{tripId}/accommodation")]
    [ProducesResponseType(typeof(AccommodationDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AccommodationDto>> AddAccommodation(Guid tripId, [FromBody] CreateAccommodationRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _accommodationService.AddAccommodationAsync(request, tripId, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Get a paginated list of trips for the authenticated user
    /// </summary>
    /// <param name="request">Query parameters for pagination, sorting, and filtering</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Paginated list of trips</returns>
    /// <response code="200">Trips retrieved successfully</response>
    /// <response code="400">Invalid query parameters</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(TripListResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TripListResponse>> ListTrips([FromQuery] TripListRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _tripService.GetTripsAsync(request, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Update an existing trip
    /// </summary>
    /// <param name="tripId">The ID of the trip to update</param>
    /// <param name="request">Trip update data (partial update)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated trip information</returns>
    /// <response code="200">Trip updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - trip belongs to another user</response>
    /// <response code="404">Trip not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("{tripId}")]
    [ProducesResponseType(typeof(TripDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TripDetailDto>> UpdateTrip(Guid tripId, [FromBody] UpdateTripRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _tripService.UpdateTripAsync(tripId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Add transportation details to an existing trip
    /// </summary>
    /// <param name="tripId">The ID of the trip to add transport to</param>
    /// <param name="request">Transportation details to add</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Created transportation information</returns>
    /// <response code="201">Transport created successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - trip belongs to another user</response>
    /// <response code="404">Trip not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("{tripId}/transports")]
    [ProducesResponseType(typeof(CreateTransportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CreateTransportResponse>> CreateTransport(Guid tripId, [FromBody] CreateTransportRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _transportService.CreateTransportAsync(tripId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }
}