using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Transport;
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

    /// <summary>
    /// Retrieves a paginated list of trips for the authenticated user
    /// </summary>
    /// <param name="request">The list request parameters</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The paginated trip list or an error</returns>
    Task<Result<TripListResponse>> GetTripsAsync(TripListRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing trip for the authenticated user
    /// </summary>
    /// <param name="tripId">The ID of the trip to update</param>
    /// <param name="request">The trip update request with optional fields</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated trip data or an error</returns>
    Task<Result<TripDetailDto>> UpdateTripAsync(Guid tripId, UpdateTripRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates transportation details for an existing trip
    /// </summary>
    /// <param name="tripId">The ID of the trip to add transport to</param>
    /// <param name="request">The transport creation request</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transport data or an error</returns>
    Task<Result<CreateTransportResponse>> CreateTransportAsync(Guid tripId, CreateTransportRequest request, Guid userId, CancellationToken cancellationToken = default);
}