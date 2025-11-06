using Trivare.Application.DTOs.Common;
using Trivare.Application.DTOs.Transport;

namespace Trivare.Application.Interfaces;

/// <summary>
/// Service for transport-related operations
/// </summary>
public interface ITransportService
{
    /// <summary>
    /// Creates transportation details for an existing trip
    /// </summary>
    /// <param name="tripId">The ID of the trip to add transport to</param>
    /// <param name="request">The transport creation request</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transport data or an error</returns>
    Task<Result<CreateTransportResponse>> CreateTransportAsync(Guid tripId, CreateTransportRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates transportation details for an existing transport record
    /// </summary>
    /// <param name="transportId">The ID of the transport to update</param>
    /// <param name="request">The transport update request with optional fields</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated transport data or an error</returns>
    Task<Result<TransportResponse>> UpdateTransportAsync(Guid transportId, UpdateTransportRequest request, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all transports for a specific trip
    /// </summary>
    /// <param name="tripId">The ID of the trip to get transports for</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The list of transports or an error</returns>
    Task<Result<IEnumerable<TransportResponse>>> GetTransportsAsync(Guid tripId, Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a transport record
    /// </summary>
    /// <param name="transportId">The ID of the transport to delete</param>
    /// <param name="userId">The ID of the authenticated user</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if deleted successfully, false otherwise, or an error</returns>
    Task<Result<bool>> DeleteTransportAsync(Guid transportId, Guid userId, CancellationToken cancellationToken = default);
}