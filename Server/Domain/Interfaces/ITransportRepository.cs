using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for Transport entity operations
/// </summary>
public interface ITransportRepository
{
    /// <summary>
    /// Gets a transport by its ID
    /// </summary>
    /// <param name="transportId">The transport ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transport entity or null if not found</returns>
    Task<Transport?> GetByIdAsync(Guid transportId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the transports for a specific trip
    /// </summary>
    /// <param name="tripId">The trip ID to get transports for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The list of transport entities</returns>
    Task<IEnumerable<Transport>> GetByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new transport
    /// </summary>
    /// <param name="transport">The transport entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transport with generated ID</returns>
    Task<Transport> AddAsync(Transport transport, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing transport
    /// </summary>
    /// <param name="transport">The transport entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated transport</returns>
    Task<Transport> UpdateAsync(Transport transport, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes a transport
    /// </summary>
    /// <param name="transport">The transport entity to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    Task DeleteAsync(Transport transport, CancellationToken cancellationToken = default);
}