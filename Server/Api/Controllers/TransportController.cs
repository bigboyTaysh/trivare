using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Trivare.Api.Controllers.Utils;
using Trivare.Api.Extensions;
using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Transport;
using Trivare.Application.Interfaces;

namespace Trivare.Api.Controllers;

/// <summary>
/// Controller for transport operations
/// </summary>
[ApiController]
[Route("api/transport")]
[Authorize]
[Produces("application/json")]
public class TransportController : ControllerBase
{
    private readonly ITransportService _transportService;

    public TransportController(ITransportService transportService)
    {
        _transportService = transportService;
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
    [HttpPost("~/api/trips/{tripId}/transports")]
    [ProducesResponseType(typeof(CreateTransportResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateTransport(Guid tripId, CreateTransportRequest request, CancellationToken cancellationToken = default)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _transportService.CreateTransportAsync(tripId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }

    /// <summary>
    /// Update transportation details for an existing transport record
    /// </summary>
    /// <param name="transportId">The ID of the transport to update</param>
    /// <param name="request">Transportation update data (partial update)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Updated transportation information</returns>
    /// <response code="200">Transport updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="401">Unauthorized - invalid or missing JWT token</response>
    /// <response code="403">Forbidden - transport belongs to another user</response>
    /// <response code="404">Transport not found</response>
    /// <response code="500">Internal server error</response>
    [HttpPatch("{transportId}")]
    [ProducesResponseType(typeof(TransportResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<TransportResponse>> UpdateTransport(Guid transportId, [FromBody] UpdateTransportRequest request, CancellationToken cancellationToken)
    {
        var userId = this.GetAuthenticatedUserId();
        var result = await _transportService.UpdateTransportAsync(transportId, request, userId, cancellationToken);

        return this.HandleResult(result);
    }
}