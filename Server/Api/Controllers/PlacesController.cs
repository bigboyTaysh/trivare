using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Places;
using Trivare.Application.Interfaces;
using Trivare.Domain.Interfaces;

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
    private readonly IGooglePlacesService _googlePlacesService;
    private readonly ILogger<PlacesController> _logger;

    public PlacesController(
        IPlacesService placesService, 
        IGooglePlacesService googlePlacesService,
        ILogger<PlacesController> logger)
    {
        _placesService = placesService;
        _googlePlacesService = googlePlacesService;
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
    /// Get autocomplete predictions for place names and addresses
    /// Uses Google Places API to provide location suggestions as user types
    /// Returns up to 5 predictions matching the input
    /// </summary>
    /// <param name="input">User input to autocomplete (minimum 3 characters)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of autocomplete predictions</returns>
    /// <response code="200">Autocomplete predictions retrieved successfully</response>
    /// <response code="400">Invalid input or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error - external API failure</response>
    [HttpGet("autocomplete")]
    [ProducesResponseType(typeof(AutocompleteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<AutocompleteResponse>> GetAutocomplete(
        [FromQuery] string input,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(input) || input.Length < 3)
        {
            return BadRequest(new ErrorResponse
            {
                Error = "InvalidInput",
                Message = "Input must be at least 3 characters long"
            });
        }

        try
        {
            var predictions = await _googlePlacesService.GetAutocompletePredictionsAsync(input, cancellationToken);

            var response = new AutocompleteResponse
            {
                Predictions = predictions.Select(p => new AutocompletePredictionDto
                {
                    Description = p.Description,
                    PlaceId = p.PlaceId,
                    StructuredFormatting = p.StructuredFormatting != null ? new AutocompleteStructuredFormatDto
                    {
                        MainText = p.StructuredFormatting.MainText,
                        SecondaryText = p.StructuredFormatting.SecondaryText
                    } : null
                }).ToList()
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting autocomplete predictions for input: {Input}", input);
            return StatusCode(StatusCodes.Status500InternalServerError, new ErrorResponse
            {
                Error = "InternalError",
                Message = "Failed to get autocomplete predictions"
            });
        }
    }

    /// <summary>
    /// Proxy endpoint to fetch Google Places photos
    /// Prevents exposing the API key on the frontend by streaming the image through the backend
    /// </summary>
    /// <param name="photoReference">Photo reference from Google Places API</param>
    /// <param name="maxWidth">Maximum width of the photo (default: 400)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The image stream</returns>
    /// <response code="200">Returns the image</response>
    /// <response code="400">Invalid photo reference</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="404">Photo not found</response>
    [HttpGet("photo")]
    [AllowAnonymous] // Allow anonymous access since images are public content
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPlacePhoto(
        [FromQuery] string photoReference,
        [FromQuery] int maxWidth = 400,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(photoReference))
        {
            return BadRequest(new ErrorResponse
            {
                Error = "InvalidPhotoReference",
                Message = "Photo reference is required"
            });
        }

        try
        {
            // Generate the Google Photos URL with API key
            var photoUrl = _googlePlacesService.GetPhotoUrl(photoReference, maxWidth);
            
            // Create HttpClient to fetch the image
            using var httpClient = new HttpClient();
            var response = await httpClient.GetAsync(photoUrl, cancellationToken);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to fetch photo from Google: {StatusCode}", response.StatusCode);
                return NotFound();
            }

            // Get the image stream and content type
            var imageStream = await response.Content.ReadAsStreamAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString() ?? "image/jpeg";
            
            // Stream the image back to the client
            return File(imageStream, contentType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching photo for reference: {PhotoReference}", photoReference);
            return NotFound();
        }
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
