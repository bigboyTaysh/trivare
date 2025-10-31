using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Places;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for AI-powered place search operations
/// </summary>
[ApiController]
[Route("api/places")]
[Authorize]
[Produces("application/json")]
public class PlacesController : ControllerBase
{
    private readonly IPlacesService _placesService;
    private readonly ILogger<PlacesController> _logger;

    public PlacesController(IPlacesService placesService, ILogger<PlacesController> logger)
    {
        _placesService = placesService;
        _logger = logger;
    }

    /// <summary>
    /// Search for places using AI-powered filtering and ranking
    /// Integrates Google Places API with OpenRouter.ai to find relevant places
    /// Returns up to 8 places matching the search criteria
    /// </summary>
    /// <param name="location">Location to search in (e.g., "Paris, France")</param>
    /// <param name="keyword">Search keyword (e.g., "museum", "breakfast with coffee")</param>
    /// <param name="preferences">Optional preferences for AI filtering (e.g., "vegetarian-friendly")</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of up to 8 filtered and ranked places</returns>
    /// <response code="200">Places retrieved successfully</response>
    /// <response code="400">Invalid search parameters or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error - external API failure or unexpected error</response>
    [HttpGet("search")]
    [ProducesResponseType(typeof(PlaceSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlaceSearchResponse>> SearchPlaces(
        [FromQuery] string location,
        [FromQuery] string keyword,
        [FromQuery] string? preferences = null,
        CancellationToken cancellationToken = default)
    {
        var request = new PlaceSearchRequest
        {
            Location = location,
            Keyword = keyword,
            Preferences = preferences
        };

        var userId = this.GetAuthenticatedUserId();
        var result = await _placesService.SearchPlacesAsync(request, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Adds a place to a specific day in the user's trip itinerary
    /// Supports adding existing places or creating new manual places
    /// Validates ownership and prevents duplicates
    /// </summary>
    /// <param name="dayId">ID of the day to add the place to</param>
    /// <param name="request">Request containing place details and order</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created day-place association</returns>
    /// <response code="201">Place added to day successfully</response>
    /// <response code="400">Invalid place data or order</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - day belongs to another user</response>
    /// <response code="404">Day or place not found</response>
    /// <response code="409">Place already added to this day</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("~/api/days/{dayId}/places")]
    [ProducesResponseType(typeof(DayAttractionDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<DayAttractionDto>> AddPlaceToDay(
        Guid dayId,
        [FromBody] AddPlaceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _placesService.AddPlaceToDayAsync(dayId, request, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Updates the order or visited status of a place in a day's itinerary
    /// Supports partial updates - only provided fields are modified
    /// Requires authentication and ownership of the trip
    /// </summary>
    /// <param name="dayId">ID of the day containing the place</param>
    /// <param name="placeId">ID of the place to update</param>
    /// <param name="request">Request containing fields to update (order and/or isVisited)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated day-place association data</returns>
    /// <response code="200">Place updated successfully</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - day belongs to another user</response>
    /// <response code="404">Day not found or place not associated with day</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("~/api/days/{dayId}/places/{placeId}")]
    [ProducesResponseType(typeof(UpdateDayAttractionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UpdateDayAttractionResponse>> UpdatePlaceOnDay(
        Guid dayId,
        Guid placeId,
        [FromBody] UpdateDayAttractionRequest request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _placesService.UpdatePlaceOnDayAsync(dayId, placeId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Removes a place from a specific day in the user's trip itinerary
    /// Validates ownership and ensures the association exists
    /// Reorders remaining places to maintain proper sequence
    /// </summary>
    /// <param name="dayId">ID of the day containing the place</param>
    /// <param name="placeId">ID of the place to remove</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>No content on successful removal</returns>
    /// <response code="204">Place removed successfully</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - day belongs to another user</response>
    /// <response code="404">Day not found or place not associated with day</response>
    /// <response code="500">Internal server error</response>
    [HttpDelete("~/api/days/{dayId}/places/{placeId}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemovePlaceFromDay(
        Guid dayId,
        Guid placeId,
        CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _placesService.RemovePlaceFromDayAsync(dayId, placeId, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Updates place details
    /// Supports partial updates - only provided fields are modified
    /// Requires authentication and ownership of a trip containing this place
    /// </summary>
    /// <param name="placeId">ID of the place to update</param>
    /// <param name="request">Request containing fields to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated place details</returns>
    /// <response code="200">Place updated successfully</response>
    /// <response code="400">Invalid input data or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - user does not own any trip containing this place</response>
    /// <response code="404">Place not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("{placeId}")]
    [ProducesResponseType(typeof(PlaceDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlaceDto>> UpdatePlace(
        Guid placeId,
        [FromBody] UpdatePlaceRequest request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _placesService.UpdatePlaceAsync(placeId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }
}
