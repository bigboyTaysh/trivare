using Trivare.Domain.Entities;

namespace Trivare.Domain.Interfaces;

/// <summary>
/// Repository for Trip entity operations
/// </summary>
public interface ITripRepository
{
    /// <summary>
    /// Creates a new trip
    /// </summary>
    /// <param name="trip">The trip entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created trip with generated ID</returns>
    Task<Trip> AddAsync(Trip trip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a trip by its ID
    /// </summary>
    /// <param name="tripId">The trip ID to retrieve</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The trip entity or null if not found</returns>
    Task<Trip?> GetByIdAsync(Guid tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing trip
    /// </summary>
    /// <param name="trip">The trip entity to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated trip</returns>
    Task<Trip> UpdateAsync(Trip trip, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of trips for a specific user
    /// </summary>
    /// <param name="userId">The user ID to count trips for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The number of trips for the user</returns>
    Task<int> CountByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets a paginated list of trips with filtering and sorting applied
    /// </summary>
    /// <param name="userId">The user ID to filter trips for</param>
    /// <param name="search">Search term for filtering by name, destination, notes, start date, or end date</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc or desc)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of trips collection and total count</returns>
    Task<(IEnumerable<Trip> Trips, int TotalCount)> GetTripsPaginatedAsync(Guid userId, string? search, string sortBy, string sortOrder, int page, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets the transport for a specific trip
    /// </summary>
    /// <param name="tripId">The trip ID to get transport for</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The transport entity or null if not found</returns>
    Task<Transport?> GetTransportByTripIdAsync(Guid tripId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Creates a new transport
    /// </summary>
    /// <param name="transport">The transport entity to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created transport with generated ID</returns>
    Task<Transport> AddTransportAsync(Transport transport, CancellationToken cancellationToken = default);
}