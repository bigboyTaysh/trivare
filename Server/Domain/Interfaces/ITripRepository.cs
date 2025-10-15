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
    /// Gets all trips for a specific user
    /// </summary>
    /// <param name="userId">The user ID to filter trips</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Collection of trips for the user</returns>
    Task<ICollection<Trip>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

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
    /// <param name="search">Search term for filtering by name, destination, notes, start date, or end date</param>
    /// <param name="sortBy">Sort field</param>
    /// <param name="sortOrder">Sort order (asc or desc)</param>
    /// <param name="page">Page number (1-based)</param>
    /// <param name="pageSize">Number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple of trips collection and total count</returns>
    Task<(IEnumerable<Trip> Trips, int TotalCount)> GetTripsPaginatedAsync(string? search, string sortBy, string sortOrder, int page, int pageSize, CancellationToken cancellationToken = default);
}