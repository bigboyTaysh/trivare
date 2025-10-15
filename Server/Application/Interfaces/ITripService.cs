using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Trips;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for trip-related operations
/// </summary>
public interface ITripService
{
    /// <summary>
    /// Creates a new trip for the authenticated user
    /// </summary>
    /// <param name="request">The trip creation request</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created trip data or an error</returns>
    Task<Result<CreateTripResponse>> CreateTripAsync(CreateTripRequest request, Guid userId, CancellationToken cancellationToken = default);
}