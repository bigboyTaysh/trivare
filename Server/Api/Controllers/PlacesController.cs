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
    /// Returns up to 5 places matching the search criteria
    /// </summary>
    /// <param name="request">Search parameters including location, keyword, and optional preferences</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of up to 5 filtered and ranked places</returns>
    /// <response code="200">Places retrieved successfully</response>
    /// <response code="400">Invalid search parameters or validation errors</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="500">Internal server error - external API failure or unexpected error</response>
    [HttpPost("search")]
    [ProducesResponseType(typeof(PlaceSearchResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<PlaceSearchResponse>> SearchPlaces(
        [FromBody] PlaceSearchRequest request,
        CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _placesService.SearchPlacesAsync(request, userId, cancellationToken);

        return this.HandleResult(result);
    }
}
