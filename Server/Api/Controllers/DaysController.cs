using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Days;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Places;
using Trivare.Application.Interfaces;
using System.Net;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for day-related operations
/// </summary>
[ApiController]
[Route("api/trips/{tripId}/days")]
[Authorize]
public class DaysController : ControllerBase
{
    private readonly IDayService _dayService;

    public DaysController(IDayService dayService)
    {
        _dayService = dayService;
    }

    /// <summary>
    /// Creates a new day for a specific trip
    /// Validates that the day falls within the trip's date range and ensures no duplicate dates exist
    /// </summary>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="request">The create day request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created day details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(CreateDayResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateDay(Guid tripId, CreateDayRequest request, CancellationToken cancellationToken = default)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _dayService.CreateDayAsync(request, tripId, userId, cancellationToken);

        return this.HandleResult(result, StatusCodes.Status201Created);
    }

    /// <summary>
    /// Retrieves all days for a specific trip with associated places
    /// </summary>
    /// <param name="tripId">The ID of the trip</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of days with places for the trip</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DayWithPlacesDto>), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetDays(Guid tripId, CancellationToken cancellationToken = default)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _dayService.GetDaysForTripAsync(tripId, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Updates the details of a specific day
    /// Validates ownership and ensures no duplicate dates within the trip
    /// </summary>
    /// <param name="dayId">The ID of the day to update</param>
    /// <param name="request">The update day request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated day details</returns>
    [HttpPatch("{dayId}")]
    [Route("api/days/{dayId}")]
    [ProducesResponseType(typeof(DayDto), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> UpdateDay(Guid dayId, UpdateDayRequest request, CancellationToken cancellationToken = default)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _dayService.UpdateDayAsync(dayId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }
}